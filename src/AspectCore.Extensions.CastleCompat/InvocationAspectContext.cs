using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Castle.DynamicProxy;

namespace AspectCore.Extensions.CastleCompat
{
    /// <summary>
    /// A minimal <see cref="AspectContext"/> implementation backed by a Castle <see cref="IInvocation"/>.
    /// Used by <see cref="AspectCoreInterceptorAdapter"/> to present an AspectContext to AspectCore interceptors.
    /// </summary>
    internal sealed class InvocationAspectContext : AspectContext
    {
        private readonly IInvocation _invocation;
        private readonly Dictionary<string, object> _additionalData = new();
        private object _returnValue;

        public InvocationAspectContext(IInvocation invocation)
        {
            _invocation = invocation ?? throw new ArgumentNullException(nameof(invocation));
            _returnValue = invocation.ReturnValue;
        }

        public override IDictionary<string, object> AdditionalData => _additionalData;

        public override object ReturnValue
        {
            get => _returnValue!;
            set => _returnValue = value;
        }

        public override IServiceProvider ServiceProvider =>
            throw new NotSupportedException(
                "ServiceProvider is not available through the Castle compatibility adapter. " +
                "Use native AspectCore interceptors for DI-dependent scenarios.");

        public override MethodInfo ServiceMethod => _invocation.Method;

        public override object Implementation => _invocation.InvocationTarget;

        public override MethodInfo ImplementationMethod => _invocation.MethodInvocationTarget;

        public override object[] Parameters => _invocation.Arguments;

        public override MethodInfo ProxyMethod => _invocation.Method;

        public override MethodInfo PredicateMethod => _invocation.Method;

        public override object Proxy => _invocation.Proxy;

        public override Task Break() => Task.CompletedTask;

        public override Task Invoke(AspectDelegate next) => next(this);

        public override Task Complete() => Task.CompletedTask;

        /// <summary>
        /// Creates an <see cref="AspectDelegate"/> that invokes the Castle invocation's Proceed.
        /// </summary>
        internal static AspectDelegate CreateProceedDelegate(IInvocation invocation)
        {
            return ctx =>
            {
                invocation.Proceed();
                // Sync the return value back to the AspectContext after proceed
                ctx.ReturnValue = invocation.ReturnValue;
                return Task.CompletedTask;
            };
        }
    }
}
