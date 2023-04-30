using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Reflection;
using Castle.DynamicProxy;
using System;
using System.Reflection;
using System.Threading.Tasks;
using CastleIntercrptor = Castle.DynamicProxy.IInterceptor;

namespace AspectCore.Extensions.Windsor
{
    [NonAspect]
    public class DynamicProxyInterceptor : CastleIntercrptor
    {
        private readonly IAspectBuilderFactory _aspectBuilderFactory;
        private readonly IAspectContextFactory _aspectContextFactory;
        private readonly IAspectValidator _aspectValidator;
        private readonly IAspectConfiguration _aspectConfiguration;
        private const string IndexFieldName = "currentInterceptorIndex";

        private static readonly FieldInfo _indexFieldInfo = typeof(AbstractInvocation).GetField(IndexFieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);

        public DynamicProxyInterceptor(IAspectBuilderFactory aspectBuilderFactory,
            IAspectContextFactory aspectContextFactory, IAspectValidatorBuilder aspectValidatorBuilder,
            IAspectConfiguration aspectConfiguration)
        {
            _aspectBuilderFactory = aspectBuilderFactory ?? throw new ArgumentNullException(nameof(aspectBuilderFactory));
            _aspectContextFactory = aspectContextFactory ?? throw new ArgumentNullException(nameof(aspectContextFactory));
            _aspectValidator = aspectValidatorBuilder?.Build() ?? throw new ArgumentNullException(nameof(aspectValidatorBuilder));
            _aspectConfiguration = aspectConfiguration ?? throw new ArgumentNullException(nameof(aspectConfiguration));
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
            var activator = new AspectActivatorFactory(_aspectContextFactory, builderFactory, _aspectConfiguration).Create();
            var activatorContext = new AspectActivatorContext(invocation.Method, invocation.MethodInvocationTarget, proxyMethod, invocation.InvocationTarget, invocation.Proxy,
                invocation.Arguments);
            var reflector = InterceptUtils.GetInvokeReflector(invocation.Method);
            invocation.ReturnValue = reflector.Invoke(activator, activatorContext);
        }
    }
}