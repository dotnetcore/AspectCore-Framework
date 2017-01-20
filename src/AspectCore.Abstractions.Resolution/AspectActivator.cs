using System;
using System.Reflection;
using System.Threading.Tasks;
#if NET451
using Nito.AsyncEx;
#endif

namespace AspectCore.Abstractions.Resolution
{
    public class AspectActivator : IAspectActivator
    {

        #region Aspect metaData

        private Type serviceType;
        private MethodInfo serviceMethod;
        private MethodInfo targetMethod;
        private MethodInfo proxyMethod;

        #endregion

        #region Dependency injection

        private readonly IAspectBuilder aspectBuilder;
        private readonly IServiceProvider serviceProvider;
        private readonly IInterceptorMatcher interceptorMatcher;
        private readonly IInterceptorInjector interceptorInjector;

        #endregion

        public AspectActivator(IServiceProvider serviceProvider,
            IAspectBuilder aspectBuilder, IInterceptorMatcher interceptorMatcher, IInterceptorInjector interceptorInjector)
        {
            if (aspectBuilder == null)
            {
                throw new ArgumentNullException(nameof(aspectBuilder));
            }
            if (interceptorMatcher == null)
            {
                throw new ArgumentNullException(nameof(interceptorMatcher));
            }
            if (interceptorInjector == null)
            {
                throw new ArgumentNullException(nameof(interceptorInjector));
            }

            this.serviceProvider = serviceProvider;
            this.aspectBuilder = aspectBuilder;
            this.interceptorMatcher = interceptorMatcher;
            this.interceptorInjector = interceptorInjector;
        }

        public void InitializeMetaData(Type serviceType, MethodInfo serviceMethod, MethodInfo targetMethod, MethodInfo proxyMethod)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }
            if (serviceMethod == null)
            {
                throw new ArgumentNullException(nameof(serviceMethod));
            }
            if (targetMethod == null)
            {
                throw new ArgumentNullException(nameof(targetMethod));
            }
            if (proxyMethod == null)
            {
                throw new ArgumentNullException(nameof(proxyMethod));
            }

            this.serviceType = serviceType;
            this.serviceMethod = serviceMethod;
            this.targetMethod = targetMethod;
            this.proxyMethod = proxyMethod;
        }

        public T Invoke<T>(object targetInstance, object proxyInstance, params object[] parameters)
        {
#if NET451
            return AsyncContext.Run(() => InvokeAsync<T>(targetInstance, proxyInstance, parameters));
#else
            var invokeAsync = InvokeAsync<T>(targetInstance, proxyInstance, parameters);

            if (invokeAsync.IsCompleted)
            {
                return invokeAsync.Result;
            }

            return Task.Run(async () => await invokeAsync).GetAwaiter().GetResult();
#endif
        }

        public async Task<T> InvokeAsync<T>(object targetInstance, object proxyInstance, params object[] parameters)
        {
            var parameterCollection = new ParameterCollection(parameters, serviceMethod.GetParameters());
            var returnParameter = new ReturnParameterDescriptor(default(T), serviceMethod.ReturnParameter);
            var targetDescriptor = new TargetDescriptor(targetInstance, serviceMethod, serviceType, targetMethod, targetInstance.GetType());
            var proxyDescriptor = new ProxyDescriptor(proxyInstance, proxyMethod, proxyInstance.GetType());

            using (var context = new AspectContext(serviceProvider, targetDescriptor, proxyDescriptor, parameterCollection, returnParameter))
            {
                var interceptors = interceptorMatcher.Match(serviceMethod, serviceType.GetTypeInfo());

                foreach (var interceptor in interceptors)
                {
                    interceptorInjector.Inject(interceptor);
                    aspectBuilder.AddAspectDelegate(interceptor.Invoke);
                }

                await aspectBuilder.Build(() => context.Target.Invoke(context.Parameters))(context);
                return await ConvertReturnVaule<T>(context.ReturnParameter.Value);
            }
        }

        private async Task<T> ConvertReturnVaule<T>(object value)
        {
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
