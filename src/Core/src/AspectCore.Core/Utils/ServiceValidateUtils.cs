using System;
using System.Reflection;
using AspectCore.Abstractions;
using AspectCore.Core.Injector;
using AspectCore.Extensions.Reflection;

namespace AspectCore.Core.Utils
{
    internal static class ServiceValidateUtils
    {
        internal static bool TryValidate(ServiceDefinition definition, IAspectValidator aspectValidator, out Type implementationType)
        {
            implementationType = null;

            if (!aspectValidator.Validate(definition.ServiceType))
            {
                return false;
            }

            implementationType = definition.GetImplementationType();

            if (definition.ServiceType.GetTypeInfo().IsClass)
            {
                if(!(definition is TypeServiceDefinition))
                {
                    return false;
                }
                if (implementationType == null)
                {
                    return false;
                }
                if (!CanInherited(implementationType.GetTypeInfo()))
                {
                    return false;
                }
            }

            if (implementationType == null)
            {
                return false;
            }

            return true;

            bool CanInherited(TypeInfo typeInfo)
            {
                if (typeInfo == null)
                {
                    throw new ArgumentNullException(nameof(typeInfo));
                }

                if (!typeInfo.IsClass || typeInfo.IsSealed)
                {
                    return false;
                }

                if (typeInfo.GetReflector().IsDefined<NonAspectAttribute>() || typeInfo.GetReflector().IsDefined<DynamicallyAttribute>())
                {
                    return false;
                }

                if (typeInfo.IsNested)
                {
                    return typeInfo.IsNestedPublic && typeInfo.DeclaringType.GetTypeInfo().IsPublic;
                }
                else
                {
                    return typeInfo.IsPublic;
                }
            }
        }
    }
}
