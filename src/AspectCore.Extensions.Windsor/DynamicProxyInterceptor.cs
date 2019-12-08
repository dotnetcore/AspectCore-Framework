using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Reflection;
using Castle.DynamicProxy;
using CastleIntercrptor = Castle.DynamicProxy.IInterceptor;

namespace AspectCore.Extensions.Windsor
{
    [NonAspect]
    public class DynamicProxyInterceptor : CastleIntercrptor
    {
        private readonly IAspectBuilderFactory _aspectBuilderFactory;
        private readonly IAspectContextFactory _aspectContextFactory;
        private readonly IAspectValidator _aspectValidator;
        private readonly IAspectExceptionWrapper _aspectExceptionWrapper;
        private const string IndexFieldName = "currentInterceptorIndex";

        private static readonly FieldInfo _indexFieldInfo = typeof(AbstractInvocation).GetField(IndexFieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);

        public DynamicProxyInterceptor(IAspectBuilderFactory aspectBuilderFactory,
            IAspectContextFactory aspectContextFactory, IAspectValidatorBuilder aspectValidatorBuilder,
            IAspectExceptionWrapper aspectExceptionWrapper)
        {
            _aspectBuilderFactory = aspectBuilderFactory ?? throw new ArgumentNullException(nameof(aspectBuilderFactory));
            _aspectContextFactory = aspectContextFactory ?? throw new ArgumentNullException(nameof(aspectContextFactory));
            _aspectValidator = aspectValidatorBuilder?.Build() ?? throw new ArgumentNullException(nameof(aspectValidatorBuilder));
            _aspectExceptionWrapper = aspectExceptionWrapper ?? throw new ArgumentNullException(nameof(aspectExceptionWrapper));
        }

        public void Intercept(IInvocation invocation)
        {
            if (!_aspectValidator.Validate(invocation.Method, true) && !_aspectValidator.Validate(invocation.MethodInvocationTarget, false))
            {
                invocation.Proceed();
                return;
            }

            if (invocation.Proxy == null)
            {
                return;
            }

            var index = _indexFieldInfo.GetValue(invocation);
            var proxyTypeInfo = invocation.Proxy.GetType().GetTypeInfo();
            var builderFactory = new WindsorAspectBuilderFactory(_aspectBuilderFactory, ctx =>
            {
                _indexFieldInfo.SetValue(invocation, index);
                invocation.Proceed();
                ctx.ReturnValue = invocation.ReturnValue;
                return Task.FromResult(0);
            });
            var proxyMethod = proxyTypeInfo.GetMethodBySignature(invocation.Method);
            var activator = new AspectActivatorFactory(_aspectContextFactory, builderFactory, _aspectExceptionWrapper).Create();
            var activatorContext = new AspectActivatorContext(invocation.Method, invocation.MethodInvocationTarget, proxyMethod, invocation.InvocationTarget, invocation.Proxy,
                invocation.Arguments);
            var reflector = InterceptUtils.GetInvokeReflector(invocation.Method);
            invocation.ReturnValue = reflector.Invoke(activator, activatorContext);
        }
    }
}