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

        public AspectExecutor(IServiceProvider serviceProvider,IJoinPoint joinPoint, IAspectContextFactory aspectContextFactory)
        {
            this.serviceProvider = serviceProvider;
            this.joinPoint = joinPoint;
            this.aspectContextFactory = aspectContextFactory;
        }

        public TResult ExecuteSynchronously<TResult>(object targetInstance , object proxyInstance , Type serviceType , string method , params object[] args)
        {
            return AsyncContext.Run(() => ExecuteAsync<TResult>(targetInstance , proxyInstance , serviceType , method , args));
        }

        public async Task<TResult> ExecuteAsync<TResult>(object targetInstance, object proxyInstance, Type serviceType, string method, params object[] args)
        {
            var serviceMethod = serviceType.GetTypeInfo().GetRequiredMethod(method, args);
            var parameters = new ParameterCollection(args, serviceMethod.GetParameters());
            var returnParameter = new ReturnParameterDescriptor(default(object), serviceMethod.ReturnParameter);
            var targetMethod = targetInstance.GetType().GetTypeInfo().GetRequiredMethod(method, args);
            var target = new Target(targetMethod, serviceType, targetInstance.GetType(), targetInstance) { ParameterCollection = parameters };
            var proxyMethod = proxyInstance.GetType().GetTypeInfo().GetRequiredMethod(serviceType.GetTypeInfo().IsInterface ? $"{serviceType.FullName}.{method}" : method, args);
            var proxy = new Proxy(proxyInstance , proxyMethod , proxyInstance.GetType());
            joinPoint.MethodInvoker = target;
            var interceptors = serviceMethod.GetInterceptors();
            InterceptorInjectionFromService(interceptors, serviceProvider);
            interceptors.ForEach(item => joinPoint.AddInterceptor(next => ctx => item.ExecuteAsync(ctx, next)));
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
                    return (TResult)context.ReturnParameter.Value;
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }
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
