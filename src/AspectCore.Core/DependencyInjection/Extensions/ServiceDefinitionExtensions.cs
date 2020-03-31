using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using AspectCore.Extensions.Reflection;
using AspectCore.Utils;

namespace AspectCore.DependencyInjection
{
    internal static class ServiceDefinitionExtensions
    {
        private static readonly ConcurrentDictionary<ServiceDefinition, bool> _callbackMap = new ConcurrentDictionary<ServiceDefinition, bool>();
        
        internal static Type GetImplementationType(this ServiceDefinition serviceDefinition)
        {
            if (serviceDefinition is TypeServiceDefinition typeServiceDefinition)
            {
                return typeServiceDefinition.ImplementationType;
            }
            else if (serviceDefinition is InstanceServiceDefinition instanceServiceDefinition)
            {
                return instanceServiceDefinition.ImplementationInstance.GetType();
            }
            else if (serviceDefinition is DelegateServiceDefinition delegaetServiceDefinition)
            {
                var typeArguments = delegaetServiceDefinition.ImplementationDelegate.GetType().GenericTypeArguments;

                return typeArguments[1];
            }
            else if (serviceDefinition is ProxyServiceDefinition proxyServiceDefinition)
            {
                return proxyServiceDefinition.ProxyType;
            }

            return null;
        }

        internal static bool RequiredPropertyInjection(this ServiceDefinition serviceDefinition)
        {
            if (serviceDefinition is ProxyServiceDefinition proxyServiceDefinition && proxyServiceDefinition.ServiceType.GetTypeInfo().IsInterface)
            {
                return false;
            }
            var implType = serviceDefinition.GetImplementationType();
            if (implType == null)
            {
                return false;
            }
            if (implType == typeof(object))
            {
                return true;
            }
            return PropertyInjectionUtils.TypeRequired(implType);
        }

        internal static bool IsManyEnumerable(this ServiceDefinition serviceDefinition)
        {
            if (serviceDefinition == null)
            {
                return false;
            }
            var serviceTypeInfo = serviceDefinition.ServiceType.GetTypeInfo();
            return serviceTypeInfo.IsGenericType && serviceTypeInfo.GetGenericTypeDefinition() == typeof(IManyEnumerable<>);
        }

        internal static bool RequiredResolveCallback(this ServiceDefinition serviceDefinition)
        {
            return _callbackMap.GetOrAdd(serviceDefinition, service => !service.ServiceType.GetReflector().IsDefined<NonCallback>());
        }
    }
}