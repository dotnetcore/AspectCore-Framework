using System;
using System.Reflection;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Reflection;

namespace AspectCore.Injector
{
    internal class ServiceValidator
    {
        private readonly IAspectValidator _aspectValidator;

        internal ServiceValidator(IAspectValidatorBuilder aspectValidatorBuilder)
        {
            _aspectValidator = aspectValidatorBuilder.Build();
        }

        internal bool TryValidate(ServiceDefinition definition, out Type implementationType)
        {
            implementationType = null;

            if (!_aspectValidator.Validate(definition.ServiceType))
            {
                return false;
            }

            implementationType = definition.GetImplementationType();

            if (definition.ServiceType.GetTypeInfo().IsClass)
            {
                if (!(definition is TypeServiceDefinition))
                {
                    return false;
                }
                if (implementationType == typeof(object))
                {
                    return false;
                }
                if (!CanInherited(implementationType.GetTypeInfo()))
                {
                    return false;
                }
            }

            if (implementationType == null || implementationType == typeof(object))
            {
                return false;
            }

            if (!implementationType.GetTypeInfo().IsClass)
            {
                return false;
            }

            if (!implementationType.GetTypeInfo().IsVisible())
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