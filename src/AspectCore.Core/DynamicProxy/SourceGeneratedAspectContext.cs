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
        private readonly IServiceProvider _serviceProvider;
        private readonly MethodInfo _implementationMethod;
        private readonly object _implementation;
        private readonly IAspectInvokeDelegate _invokeDelegate;
        private bool _disposedValue = false;

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

        public override object ReturnValue { get; set; }

        public override MethodInfo ServiceMethod { get; }

        public override object[] Parameters { get; }

        public override MethodInfo ProxyMethod { get; }

        /// <summary>
        /// Gets the method used to evaluate configured <see cref="AspectCore.Configuration.AspectPredicate"/> filters.
        /// </summary>
        public override MethodInfo PredicateMethod { get; }

        public override object Proxy { get; }

        public override MethodInfo ImplementationMethod => _implementationMethod;

        public override object Implementation => _implementation;

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
            ServiceMethod = serviceMethod;
            ProxyMethod = proxyMethod;
            Proxy = proxyInstance;
            Parameters = parameters;
            PredicateMethod = predicateMethod;
        }

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
                    var reflector = AspectContextRuntimeExtensions.reflectorTable.GetOrAdd(
                        _implementationMethod,
                        method => method.GetReflector(CallOptions.Call));
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
