using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Lite.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Lite.DependencyInjection.Internal
{
    internal sealed class SupportProxyService : ISupportProxyService
    {
        private static readonly MethodInfo GetOpenIEnumerableServiceMethod =
            typeof(SupportProxyService).GetTypeInfo()
                .GetMethod("GetOpenIEnumerableService", BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly IProxyServiceProvider proxyServiceProvider;
        private readonly ISupportOriginalService supportOriginalService;
        private readonly IProxyActivator proxyActivator;

        public SupportProxyService(IProxyServiceProvider proxyServiceProvider,
            IProxyActivator proxyActivator,
            ISupportOriginalService supportOriginalService)
        {
            this.proxyServiceProvider = proxyServiceProvider;
            this.proxyActivator = proxyActivator;
            this.supportOriginalService = supportOriginalService;
        }

        public object GetService(Type serviceType, object originalServiceInstance)
        {
            if (originalServiceInstance == null)
            {
                return null;
            }

            ExceptionHelper.ThrowArgumentNull(serviceType, nameof(serviceType));
            var serviceTypeInfo = serviceType.GetTypeInfo();

            if (serviceTypeInfo.IsGenericType && serviceTypeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var genericParameter = serviceTypeInfo.GenericTypeArguments[0];
                return GetOpenIEnumerableServiceMethod.MakeGenericMethod(genericParameter)
                    .Invoke(this, new[] {genericParameter, originalServiceInstance});
            }

            return CreateServiceProxy(originalServiceInstance, serviceType);
        }

        private IEnumerable<T> GetOpenIEnumerableService<T>(Type serviceType, IEnumerable<T> enumerable)
        {
            return (from service in enumerable
                select (T) CreateServiceProxy(service, serviceType)).ToArray();
        }

        private object CreateServiceProxy(object instance, Type serviceType)
        {
            try
            {
                var serviceTypeInfo = serviceType.GetTypeInfo();

                if (!serviceTypeInfo.CanProxy(supportOriginalService))
                {
                    return ActivatorUtilities.CreateInstance(proxyServiceProvider, instance.GetType());;
                }

                var resolvedInstance = ActivatorUtilities.CreateInstance(proxyServiceProvider, instance.GetType());

                if (serviceTypeInfo.IsClass)
                {
                    return proxyActivator.CreateClassProxy(serviceType, resolvedInstance,
                        serviceTypeInfo.GetInterfaces());
                }

                if (serviceTypeInfo.IsInterface)
                {
                    return proxyActivator.CreateInterfaceProxy(serviceType, resolvedInstance,
                        serviceTypeInfo.GetInterfaces().Where(x => x != serviceType).ToArray());
                }

                return resolvedInstance;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"Unable to create proxy instance for type '{serviceType}'.", exception);
            }
        }
    }
}
