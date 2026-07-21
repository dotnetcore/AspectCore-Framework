using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AspectCore.Extensions.Reflection;
using AspectCore.Utils;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// An <see cref="AspectContext"/> implementation that uses a source-generated
    /// <see cref="IAspectInvokeDelegate"/> for target method invocation, avoiding
    /// <see cref="System.Reflection.Emit"/> and expression tree compilation.
    /// This enables NativeAOT-compatible AOP interception.
    /// </summary>
    [NonAspect]
    internal sealed class SourceGeneratedAspectContext : AspectContext, IDisposable
    {
        private volatile IDictionary<string, object> _data;
        private IServiceProvider _serviceProvider;
        private MethodInfo _implementationMethod;
        private object _implementation;
        private IAspectInvokeDelegate _invokeDelegate;
        private bool _disposedValue = false;

        // Backing fields for properties that need to be reset during pooling.
        private MethodInfo _serviceMethod;
        private object[] _parameters;
        private MethodInfo _proxyMethod;
        private MethodInfo _predicateMethod;
        private object _proxy;
        private object _returnValue;

        public override IServiceProvider ServiceProvider
        {
            get
            {
                if (_serviceProvider == null)
                {
                    throw new NotSupportedException("The current context does not support IServiceProvider.");
                }

                return _serviceProvider;
            }
        }

        public override IDictionary<string, object> AdditionalData
        {
            get
            {
                if (_data == null)
                {
                    _data = new Dictionary<string, object>();
                }
                return _data;
            }
        }

        public override object ReturnValue
        {
            get => _returnValue;
            set => _returnValue = value;
        }

        public override MethodInfo ServiceMethod => _serviceMethod;

        public override object[] Parameters => _parameters;

        public override MethodInfo ProxyMethod => _proxyMethod;

        /// <summary>
        /// Gets the method used to evaluate configured <see cref="AspectCore.Configuration.AspectPredicate"/> filters.
        /// </summary>
        public override MethodInfo PredicateMethod => _predicateMethod;

        public override object Proxy => _proxy;

        public override MethodInfo ImplementationMethod => _implementationMethod;

        public override object Implementation => _implementation;

#if NET8_0_OR_GREATER
        /// <summary>
        /// Parameterless constructor used by the object pool.
        /// Must call <see cref="Reset"/> before use.
        /// </summary>
        internal SourceGeneratedAspectContext()
        {
        }
#endif

        public SourceGeneratedAspectContext(
            IServiceProvider serviceProvider,
            MethodInfo serviceMethod,
            MethodInfo targetMethod,
            MethodInfo proxyMethod,
            MethodInfo predicateMethod,
            object targetInstance,
            object proxyInstance,
            object[] parameters,
            IAspectInvokeDelegate invokeDelegate)
        {
            _serviceProvider = serviceProvider;
            _implementationMethod = targetMethod;
            _implementation = targetInstance;
            _invokeDelegate = invokeDelegate ?? throw new ArgumentNullException(nameof(invokeDelegate));
            _serviceMethod = serviceMethod;
            _proxyMethod = proxyMethod;
            _proxy = proxyInstance;
            _parameters = parameters;
            _predicateMethod = predicateMethod;
        }

#if NET8_0_OR_GREATER
        /// <summary>
        /// Reinitializes this pooled instance with new context data.
        /// Called by <see cref="AspectContextFactory"/> after retrieving from the pool.
        /// </summary>
        internal void Reset(
            IServiceProvider serviceProvider,
            MethodInfo serviceMethod,
            MethodInfo targetMethod,
            MethodInfo proxyMethod,
            MethodInfo predicateMethod,
            object targetInstance,
            object proxyInstance,
            object[] parameters,
            IAspectInvokeDelegate invokeDelegate)
        {
            _serviceProvider = serviceProvider;
            _implementationMethod = targetMethod;
            _implementation = targetInstance;
            _invokeDelegate = invokeDelegate;
            _serviceMethod = serviceMethod;
            _proxyMethod = proxyMethod;
            _proxy = proxyInstance;
            _parameters = parameters;
            _predicateMethod = predicateMethod;
            _returnValue = null;
            _disposedValue = false;
            // _data is lazily initialized, leave null
        }

        /// <summary>
        /// Clears all references to allow safe return to the object pool.
        /// Disposes any <see cref="IDisposable"/> values in <see cref="AdditionalData"/>.
        /// </summary>
        internal void Clear()
        {
            // Dispose AdditionalData values (same logic as Dispose)
            if (_data != null)
            {
                foreach (var key in _data.Keys.ToArray())
                {
                    _data.TryGetValue(key, out object value);
                    (value as IDisposable)?.Dispose();
                    _data.Remove(key);
                }
                _data = null;
            }

            // Null out all references to prevent holding them alive in pool
            _serviceProvider = null;
            _implementationMethod = null;
            _implementation = null;
            _invokeDelegate = null;
            _serviceMethod = null;
            _proxyMethod = null;
            _proxy = null;
            _parameters = null;
            _predicateMethod = null;
            _returnValue = null;
            _disposedValue = true;
        }
#endif

        public override async Task Complete()
        {
            if (_implementation == null || _implementationMethod == null)
            {
                await Break();
                return;
            }
            object returnValue;
            if (_implementationMethod.IsGenericMethod)
            {
                // For generic methods, type arguments are erased at the IAspectInvokeDelegate.Invoke
                // level (object instance, object[] parameters). The _implementationMethod stored in
                // this context is already the closed generic MethodInfo (constructed via
                // MakeGenericMethod in the proxy body).
                //
                // When dynamic code is not supported (NativeAOT), fall back to MethodInfo.Invoke.
                // When dynamic code is supported, use the MethodReflector with CallOptions.Call
                // for non-virtual dispatch (same approach as RuntimeAspectContext).
                if (!RuntimeFeature.IsDynamicCodeSupported)
                {
                    returnValue = _implementationMethod.Invoke(_implementation, Parameters);
                }
                else
                {
                    if (!AspectContextRuntimeExtensions.reflectorTable.TryGetValue(_implementationMethod, out var reflector))
                    {
                        reflector = AspectContextRuntimeExtensions.reflectorTable.GetOrAdd(
                            _implementationMethod,
                            static method => method.GetReflector(CallOptions.Call));
                    }
                    returnValue = reflector.Invoke(_implementation, Parameters);
                }
            }
            else
            {
                // Non-generic methods (both interface and class proxies) use the
                // statically-generated delegate. For class proxies, the delegate calls
                // a base-call trampoline to avoid virtual dispatch recursion.
                returnValue = _invokeDelegate.Invoke(_implementation, Parameters);
            }
            await AwaitIfAsyncNativeAotSafe(returnValue);
            ReturnValue = returnValue;
        }

        public override Task Break()
        {
            if (ReturnValue == null)
            {
                // For ref/ref readonly returns the return parameter type is a managed
                // pointer (T&); unwrap it so the default value matches the element type
                // that the value-based pipeline materialises.
                var returnType = ServiceMethod.ReturnParameter.ParameterType;
                if (returnType.IsByRef)
                {
                    returnType = returnType.GetElementType();
                }
                ReturnValue = returnType.GetDefaultValue();
            }
            return TaskUtils.CompletedTask;
        }

        public override Task Invoke(AspectDelegate next)
        {
            return next(this);
        }

        /// <summary>
        /// Awaits the return value if it is an async type (Task, ValueTask).
        /// For ValueTask&lt;T&gt;, in NativeAOT we cannot use Expression.Compile() or reliably
        /// reflect on the struct's AsTask() method after trimming.
        /// Instead, we skip awaiting ValueTask&lt;T&gt; here — the inline activation code in
        /// the proxy method body handles it via pattern matching (switch/case ValueTask&lt;T&gt;).
        /// This matches RuntimeAspectContext behavior: AwaitIfAsync awaits but ReturnValue
        /// still holds the original Task/ValueTask object for the upper layer to handle.
        /// </summary>
        private static async ValueTask AwaitIfAsyncNativeAotSafe(object returnValue)
        {
            switch (returnValue)
            {
                case null:
                    break;
                case Task task:
                    await task;
                    break;
                case ValueTask valueTask:
                    await valueTask;
                    break;
                default:
                    {
                        // ValueTask<T> is a struct; after boxing it won't match the cases above.
                        // In NativeAOT, we cannot reliably call .AsTask() via reflection
                        // (trimmer may remove the method metadata).
                        // Skip awaiting here — the proxy's inline activation switch will
                        // handle it: case ValueTask<T> vtResult: return await vtResult;
                        // This is safe because:
                        // 1. The delegate returns the ValueTask<T> immediately (not awaited in Invoke)
                        // 2. The proxy method body awaits it in the switch after Complete() returns
                        break;
                    }
            }
        }

        public void Dispose()
        {
            if (_disposedValue)
            {
                return;
            }

            if (_data == null)
            {
                _disposedValue = true;
                return;
            }

            foreach (var key in _data.Keys.ToArray())
            {
                _data.TryGetValue(key, out object value);

                var disposable = value as IDisposable;

                disposable?.Dispose();

                _data.Remove(key);
            }

            _disposedValue = true;
        }
    }
}
