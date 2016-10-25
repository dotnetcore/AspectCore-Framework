using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.Lite.Abstractions;
using System.Reflection;
using AspectCore.Lite.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx;

namespace AspectCore.Lite.Internal
{
    internal sealed class AspectExecutor: IAspectExecutor
    {
        private readonly IJoinPoint joinPoint;
        private readonly IAspectContextFactory aspectContextFactory;
        private readonly IServiceProvider serviceProvider;
        private readonly IInterceptorMatcher interceptorMatcher;
        private readonly INamedMethodMatcher namedMethodMatcher;

        public AspectExecutor(
            IServiceProvider serviceProvider ,
            IJoinPoint joinPoint ,
            IAspectContextFactory aspectContextFactory ,
            IInterceptorMatcher interceptorMatcher ,
            INamedMethodMatcher namedMethodMatcher)
        {
            this.serviceProvider = serviceProvider;
            this.joinPoint = joinPoint;
            this.aspectContextFactory = aspectContextFactory;
            this.interceptorMatcher = interceptorMatcher;
            this.namedMethodMatcher = namedMethodMatcher;
        }

        public TResult ExecuteSynchronously<TResult>(object targetInstance , object proxyInstance , Type serviceType , string method , params object[] args)
        {
            return AsyncContext.Run(() => ExecuteAsync<TResult>(targetInstance , proxyInstance , serviceType , method , args));
        }

        public async Task<TResult> ExecuteAsync<TResult>(object targetInstance , object proxyInstance , Type serviceType , string method , params object[] args)
        {
            var serviceMethod = namedMethodMatcher.Match(serviceType , method , args);
            var parameters = new ParameterCollection(args , serviceMethod.GetParameters());
            var returnParameter = new ReturnParameterDescriptor(default(object) , serviceMethod.ReturnParameter);
            var targetMethod = namedMethodMatcher.Match(targetInstance.GetType() , method , args);
            var target = new Target(targetMethod , serviceType , targetInstance.GetType() , targetInstance) { ParameterCollection = parameters };
            var proxyMethod = namedMethodMatcher.Match(proxyInstance.GetType() , GetMethodName(serviceType , method) , args);
            var proxy = new Proxy(proxyInstance , proxyMethod , proxyInstance.GetType());

            joinPoint.MethodInvoker = target;
            var interceptors = interceptorMatcher.Match(serviceMethod, serviceMethod.DeclaringType.GetTypeInfo());
            InterceptorInjectionFromService(interceptors , serviceProvider);
            interceptors.ForEach(item => joinPoint.AddInterceptor(next => ctx => item.ExecuteAsync(ctx , next)));

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
                    return await CastResult<TResult>(context.ReturnParameter.Value);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private static async Task<TResult> CastResult<TResult>(object value)
        {
            var taskResult = value as Task<TResult>;
            if (taskResult != null) return await taskResult;
            var task = value as Task;
            if (task != null) { await task; return default(TResult); }
            return (TResult)value;
        }

        private static string GetMethodName(Type serviceType , string method)
        {
            if (!serviceType.GetTypeInfo().IsInterface)
            {
                return method;
            }
            return $"{serviceType.FullName}.{method}".Replace('+' , '.');
        }

        private void InterceptorInjectionFromService(IEnumerable<IInterceptor> interceptors, IServiceProvider serviceProvider)
        {
            foreach (var interceptor in interceptors)
            {
                foreach (var property in interceptor.GetType().GetTypeInfo().DeclaredProperties)
                {
                    var fromServiceAttribute = property.GetCustomAttribute<FromServiceAttribute>();

                    if (fromServiceAttribute == null)
                    {
                        continue;
                    }

                    property.SetValue(interceptor, serviceProvider.GetService(property.PropertyType));
                }

                var injectionMethod = interceptor.GetType().GetTypeInfo().DeclaredMethods.FirstOrDefault(m => m.Name == "FromService" && !m.IsGenericMethod && !m.IsAbstract);
                                
                if (injectionMethod == null)
                {
                    continue;
                }

                injectionMethod.Invoke(interceptor, injectionMethod.GetParameters().Select(p => serviceProvider.GetRequiredService(p.ParameterType)).ToArray());
            }
        }
    }
}
