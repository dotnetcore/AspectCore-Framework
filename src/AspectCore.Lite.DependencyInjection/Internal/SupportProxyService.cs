using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Lite.DependencyInjection.Internal
{
    internal sealed class SupportProxyService : ISupportProxyService
    {
        private readonly ISupportOriginalService supportOriginalServiceProvider;
        private readonly IProxyActivator proxyActivator;
        private readonly IProxyMemorizer proxyMemorizer;

        public SupportProxyService(
            ISupportOriginalService supportOriginalServiceProvider,
            IProxyMemorizer proxyMemorizer,
            IProxyActivator proxyActivator)
        {
            this.supportOriginalServiceProvider = supportOriginalServiceProvider;
            this.proxyActivator = proxyActivator;
            this.proxyMemorizer = proxyMemorizer;
        }

        public object GetService(Type serviceType)
        {
            ExceptionHelper.ThrowArgumentNull(serviceType, nameof(serviceType));
            var serviceTypeInfo = serviceType.GetTypeInfo();

            if (serviceTypeInfo.IsGenericType && serviceTypeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return GetOpenIEnumerableService(serviceType);
            }

            return CreateServiceProxy(supportOriginalServiceProvider.GetOriginalService(serviceType), serviceType);
        }

        private IEnumerable GetOpenIEnumerableService(Type serviceType)
        {
            var services = supportOriginalServiceProvider.GetServices(serviceType);
            foreach (var service in services)
            {
                yield return CreateServiceProxy(service, serviceType);
            }
        }

        private object CreateServiceProxy(object instance, Type serviceType)
        {
            return proxyMemorizer.GetOrSetProxy(instance, (_instance, _serviceType) =>
            {
                var serviceTypeInfo = _serviceType.GetTypeInfo();
                if (serviceTypeInfo.IsClass)
                {
                    if (serviceTypeInfo.IsSealed)
                    {
                        return _instance;
                    }
                    return proxyActivator.CreateClassProxy(_serviceType, _instance,
                        serviceTypeInfo.GetInterfaces());
                }

                if (serviceTypeInfo.IsInterface)
                {
                    return proxyActivator.CreateInterfaceProxy(_serviceType, _instance,
                        serviceTypeInfo.GetInterfaces());
                }

                return _instance;
            }, serviceType);
        }
    }
}
