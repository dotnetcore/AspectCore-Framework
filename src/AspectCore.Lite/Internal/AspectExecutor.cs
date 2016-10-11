using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.Lite.Abstractions;
using System.Reflection;
using AspectCore.Lite.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Lite.Internal
{
    internal sealed class AspectExecutor
    {
        private readonly IJoinPoint joinPoint;
        private readonly IAspectContextFactory aspectContextFactory;
        private readonly IServiceProvider serviceProvider;

        public AspectExecutor(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.joinPoint = serviceProvider.GetRequiredService<IJoinPoint>();
            this.aspectContextFactory = serviceProvider.GetRequiredService<IAspectContextFactory>();
        }

        public Task<IAspectContext> Execute( object targetInstance, object proxyInstance, Type serviceType, string method, params object[] args)
        {
            var parameterTypes = args.Select(a => a.GetType()).ToArray();
            var serviceMethod = serviceType.GetRequiredMethod(method, parameterTypes);

            var parameters = new ParameterCollection(args, serviceMethod.GetParameters());
            var returnParameter = new ReturnParameterDescriptor(null, serviceMethod.ReturnParameter);

            var targetMethod = targetInstance.GetType().GetRequiredMethod(method, parameterTypes);
            var target = new Target(targetMethod, serviceType, targetInstance.GetType(), targetInstance) { ParameterCollection = parameters };

            var proxyMethod = proxyInstance.GetType().GetRequiredMethod(method, parameterTypes);
            var proxy = new Proxy(proxyInstance, proxyMethod, proxyInstance.GetType()) { ParameterCollection = parameters };

            joinPoint.MethodInvoker = target;
            var context = aspectContextFactory.Create();

            var internalContext = context as AspectContext;
            if (internalContext != null)
            {
                internalContext.Parameters = parameters;
                internalContext.ReturnParameter = returnParameter;
                internalContext.Target = target;
                internalContext.Proxy = proxy;
            }

            var interceptors = serviceMethod.GetCustomAttributes().OfType<IInterceptor>().Distinct(i => i.GetType()).OrderBy(i => i.Order).ToArray();
            InterceptorInjectionFromService(interceptors, serviceProvider);

            interceptors.ForEach(item => joinPoint.AddInterceptor(next => ctx => item.ExecuteAsync(ctx, next)));

            return joinPoint.Build()(context).ContinueWith((task, state) =>
            {
                if (task.IsFaulted) throw task.Exception;
                return (IAspectContext)state;
            }, context, TaskContinuationOptions.ExecuteSynchronously);
        }

        private void InterceptorInjectionFromService(IEnumerable<IInterceptor> interceptors, IServiceProvider serviceProvider)
        {
            foreach (var interceptor in interceptors)
            {
                foreach (var property in interceptor.GetType().GetTypeInfo().DeclaredProperties)
                {
                    var fromService = property.GetCustomAttribute<FromServiceAttribute>();
                    if (fromService == null) continue;
                    property.SetValue(interceptor, serviceProvider.GetService(property.PropertyType));
                }

                var fromServiceable = interceptor as IFromServiceable;
                if (fromServiceable == null)
                {
                    continue;
                }

                var injectionMethod = fromServiceable.GetType().GetTypeInfo().DeclaredMethods.FirstOrDefault();
                if (injectionMethod == null)
                {
                    continue;
                }

                injectionMethod.Invoke(interceptor, injectionMethod.GetParameters().Select(p => serviceProvider.GetRequiredService(p.ParameterType)).ToArray());
            }
        }
    }
}
