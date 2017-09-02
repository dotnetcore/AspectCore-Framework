using System;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using AspectCore.Extensions.Reflection;

namespace AspectCore.Injector
{
    internal static class ServiceDefinitionExtensions
    {
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

            //Debug.Assert(false, "ImplementationType, ImplementationInstance or ImplementationFactory must be non null");
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
            return implType.GetTypeInfo().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => x.CanWrite).Any(x => x.GetReflector().IsDefined<InjectAttribute>());
        }
    }
}