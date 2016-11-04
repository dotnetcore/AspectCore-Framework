using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Lite.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Lite.DependencyInjection.Internal
{
    internal sealed class SupportProxyService : ISupportProxyService
    {
        private readonly ISupportOriginalService supportOriginalServiceProvider;
        private readonly IProxyActivator proxyActivator;

        public SupportProxyService(ISupportOriginalService supportOriginalServiceProvider,
            IProxyActivator proxyActivator)
        {
            this.supportOriginalServiceProvider = supportOriginalServiceProvider;
            this.proxyActivator = proxyActivator;
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
            var serviceTypeInfo = serviceType.GetTypeInfo();

            if (!serviceTypeInfo.CanProxy(supportOriginalServiceProvider))
            {
                return instance;
            }

            if (serviceTypeInfo.IsClass)
            {
                return proxyActivator.CreateClassProxy(serviceType, instance,
                    serviceTypeInfo.GetInterfaces());
            }

            if (serviceTypeInfo.IsInterface)
            {
                return proxyActivator.CreateInterfaceProxy(serviceType, instance,
                    serviceTypeInfo.GetInterfaces().Where(x => x != serviceType).ToArray());
            }

            return instance;
        }
    }
}
