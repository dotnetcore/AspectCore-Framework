using System;
using System.Threading.Tasks;
using AspectCore.Lite.Abstractions;
using System.Reflection;
using AspectCore.Lite.Extensions;
using Nito.AsyncEx;
using AspectCore.Lite.Internal.Generators;

namespace AspectCore.Lite.Internal
{
    internal sealed class AspectExecutor : IAspectActivator
    {
        private readonly IJoinPoint joinPoint;
        private readonly IAspectContextFactory aspectContextFactory;
        private readonly IServiceProvider serviceProvider;
        private readonly IInterceptorMatcher interceptorMatcher;
        private readonly INamedMethodMatcher namedMethodMatcher;
        private readonly IPropertyInjector propertyInjector;

        public AspectExecutor(
            IServiceProvider serviceProvider ,
            IJoinPoint joinPoint ,
            IAspectContextFactory aspectContextFactory ,
            IInterceptorMatcher interceptorMatcher ,
            INamedMethodMatcher namedMethodMatcher,
            IPropertyInjector propertyInjector)
        {
            this.serviceProvider = serviceProvider;
            this.joinPoint = joinPoint;
            this.aspectContextFactory = aspectContextFactory;
            this.interceptorMatcher = interceptorMatcher;
            this.namedMethodMatcher = namedMethodMatcher;
            this.propertyInjector = propertyInjector;
        }

        public T Invoke<T>(object targetInstance , object proxyInstance , Type serviceType , string method , params object[] args)
        {
            return AsyncContext.Run(() => ExecuteAsync<T>(targetInstance , proxyInstance , serviceType , method , args));
        }

        public async Task<T> ExecuteAsync<T>(object targetInstance , object proxyInstance , Type serviceType , string method , params object[] args)
        {
            var serviceMethod = namedMethodMatcher.Match(serviceType , method , args);
            var parameters = new ParameterCollection(args , serviceMethod.GetParameters());
            var returnParameter = new ReturnParameterDescriptor(default(T) , serviceMethod.ReturnParameter);
            var targetMethod = namedMethodMatcher.Match(targetInstance.GetType() , method , args);
            var target = new Target(targetMethod , serviceType , targetInstance.GetType() , targetInstance) { ParameterCollection = parameters };
            var proxyMethod = namedMethodMatcher.Match(proxyInstance.GetType() , GeneratorHelper.GetMethodName(serviceType , method) , args);
            var proxy = new Proxy(proxyInstance , proxyMethod , proxyInstance.GetType());
            var interceptors = interceptorMatcher.Match(serviceMethod , serviceType.GetTypeInfo());

            joinPoint.MethodInvoker = target;

            interceptors.ForEach(interceptor =>
            {
                propertyInjector.Injection(interceptor);
                joinPoint.AddInterceptor(next => ctx => interceptor.ExecuteAsync(ctx, next));
            });

            using (var context = aspectContextFactory.Create())
            {
                var internalContext = context as AspectContext;
                if (internalContext != null)
                {
                    internalContext.Parameters = parameters;
                    internalContext.ReturnParameter = returnParameter;
                    internalContext.Target = target;
                    internalContext.Proxy = proxy;
                }
                try
                {
                    await joinPoint.Build()(context);
                    return await ConvertReturnVaule<T>(context.ReturnParameter.Value);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    interceptors.ForEach(interceptor => (interceptor as IDisposable)?.Dispose());
                }
            }
        }

        private static async Task<T> ConvertReturnVaule<T>(object value)
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
