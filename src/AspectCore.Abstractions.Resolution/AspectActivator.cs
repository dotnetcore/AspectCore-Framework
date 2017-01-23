using System;
using System.Reflection;
using System.Threading.Tasks;
#if NET45
using Nito.AsyncEx;
#endif

namespace AspectCore.Abstractions.Resolution
{
    public class AspectActivator : IAspectActivator
    {
        private readonly IAspectBuilder aspectBuilder;
        private readonly IServiceProvider serviceProvider;
        private readonly IInterceptorMatcher interceptorMatcher;
        private readonly IInterceptorInjector interceptorInjector;

        public AspectActivator(
            IServiceProvider serviceProvider,
            IAspectBuilder aspectBuilder,
            IInterceptorMatcher interceptorMatcher,
            IInterceptorInjector interceptorInjector)
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

        public T Invoke<T>(AspectActivatorContext activatorContext)
        {
#if NET45
            return AsyncContext.Run(() => InvokeAsync<T>(activatorContext));
#else
            var invokeAsync = InvokeAsync<T>(activatorContext);

            if (invokeAsync.IsCompleted)
            {
                return invokeAsync.Result;
            }

            return Task.Run(async () => await invokeAsync).GetAwaiter().GetResult();
#endif
        }

        public async Task<T> InvokeAsync<T>(AspectActivatorContext activatorContext)
        {
            using (var context = new DefaultAspectContext<T>(serviceProvider, activatorContext))
            {
                var method = activatorContext.ServiceMethod;
                foreach (var interceptor in interceptorMatcher.Match(method, method.DeclaringType.GetTypeInfo()))
                {

                    interceptorInjector.Inject(interceptor);
                    aspectBuilder.AddAspectDelegate(interceptor.Invoke);

                }

                await aspectBuilder.Build(() => context.Target.Invoke(context.Parameters))(context);

                return await ConvertReturnVaule<T>(context.ReturnParameter.Value);
            }
        }
    }
}