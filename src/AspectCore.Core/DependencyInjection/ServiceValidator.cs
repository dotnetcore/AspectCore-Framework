using System;
using System.Reflection;
using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
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

            if (definition.ServiceType.GetTypeInfo().IsNonAspect())
            {
                return false;
            }

            implementationType = definition.GetImplementationType();

            if (implementationType == null || implementationType == typeof(object))
            {
                return false;
            }

            if (!implementationType.GetTypeInfo().IsClass)
            {
                return false;
            }

            if (definition.ServiceType.GetTypeInfo().IsClass)
            {
                if (!(definition is TypeServiceDefinition))
                {
                    return false;
                }
                if (!implementationType.GetTypeInfo().CanInherited())
                {
                    return false;
                }
            }

            return _aspectValidator.Validate(definition.ServiceType, true) || _aspectValidator.Validate(implementationType, false);
        }
    }
}