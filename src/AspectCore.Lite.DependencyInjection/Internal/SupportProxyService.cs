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
        private readonly IProxyServiceProvider proxyServiceProvider;
        private readonly ISupportOriginalService supportOriginalService;
        private readonly IProxyActivator proxyActivator;

        public object OriginalServiceInstance { get; set; }

        public SupportProxyService(IProxyServiceProvider proxyServiceProvider,
            IProxyActivator proxyActivator,
            ISupportOriginalService supportOriginalService)
        {
            this.proxyServiceProvider = proxyServiceProvider;
            this.proxyActivator = proxyActivator;
            this.supportOriginalService = supportOriginalService;
        }

        public object GetService(Type serviceType)
        {
            if (OriginalServiceInstance == null)
            {
                return null;
            }

            ExceptionHelper.ThrowArgumentNull(serviceType, nameof(serviceType));
            var serviceTypeInfo = serviceType.GetTypeInfo();

            if (serviceTypeInfo.IsGenericType && serviceTypeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return GetOpenIEnumerableService(serviceType);
            }

            return CreateServiceProxy(OriginalServiceInstance, serviceType);
        }

        private IEnumerable GetOpenIEnumerableService(Type serviceType)
        {
            return from object service in (IEnumerable) OriginalServiceInstance
                select CreateServiceProxy(service, serviceType);
        }

        private object CreateServiceProxy(object instance, Type serviceType)
        {
            try
            {
                var serviceTypeInfo = serviceType.GetTypeInfo();

                if (!serviceTypeInfo.CanProxy(supportOriginalService))
                {
                    return instance;
                }

                instance = ActivatorUtilities.CreateInstance(proxyServiceProvider, instance.GetType());

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
            catch (Exception exception)
            {
                throw new InvalidOperationException($"Unable to create proxy instance for type '{serviceType}'.", exception);
            }
        }
    }
}
