using AspectCore.Lite.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Nito.AsyncEx;
using AspectCore.Lite.Extensions;

namespace AspectCore.Lite.Internal
{
    internal sealed class AspectActivator : IAspectActivator
    {
        #region Aspect metaData

        private Type serviceType;
        private MethodInfo serviceMethod;
        private MethodInfo targetMethod;
        private MethodInfo proxyMethod;

        #endregion

        #region Dependency injection

        private readonly IServiceProvider serviceProvider;
        private readonly IAspectBuilder aspectBuilder;
        private readonly IInterceptorMatcher interceptorMatcher;
        private readonly IInterceptorInjector interceptorInjector;

        #endregion

        public void InitializeMetaData(Type serviceType, MethodInfo serviceMethod, MethodInfo targetMethod, MethodInfo proxyMethod)
        {
            throw new NotImplementedException();
        }

        public T Invoke<T>(object targetInstance, object proxyInstance, params object[] paramters)
        {
            return AsyncContext.Run(() => InvokeAsync<T>(targetInstance, proxyInstance, paramters));
        }

        public Task<T> InvokeAsync<T>(object targetInstance, object proxyInstance, params object[] paramters)
        {
            var parameters = new ParameterCollection(paramters, serviceMethod.GetParameters());
            var returnParameter = new ReturnParameterDescriptor(default(T), serviceMethod.ReturnParameter);
            var targetDescriptor = new TargetDescriptor(targetInstance, serviceMethod, serviceType, targetMethod, targetInstance.GetType());
            var proxyDescriptor = new ProxyDescriptor(proxyInstance, proxyMethod, proxyInstance.GetType());
            var context = new AspectContext(serviceProvider, targetDescriptor, proxyDescriptor, parameters, returnParameter);
            var interceptors = interceptorMatcher.Match(serviceMethod, serviceType.GetTypeInfo());
            foreach (var interceptor in interceptors)
            {
                interceptorInjector.Inject(interceptor);
                aspectBuilder.AddAspectDelegate(interceptor.Invoke);
            }
            return TryInvoke<T>(aspectBuilder, context, interceptors);
        }

        private async Task<T> TryInvoke<T>(IAspectBuilder aspectBuilder, AspectContext context, IEnumerable<IInterceptor> interceptors)
        {
            try
            {
                await aspectBuilder.Build(() => context.Target.Invoke(context.Parameters))(context);
                return await ConvertReturnVaule<T>(context.ReturnParameter.Value);
            }
            finally
            {
                foreach (var disposable in interceptors.OfType<IDisposable>())
                {
                    disposable.Dispose();
                }
            }
        }

        private static async Task<T> ConvertReturnVaule<T>(object value) {
            if (value is Task<T>)
            {
                return await (Task<T>)value;
            }
            else if (value is Task)
            {
                await (Task)value;
                return default(T);
            }
            else
            {
                return (T)value;
            }
        }
    }
}
