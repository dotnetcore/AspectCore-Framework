using System;
using System.Reflection;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection
{
    internal class ServiceValidator
    {
        private readonly IAspectValidator _aspectValidator;

        internal ServiceValidator(IAspectValidatorBuilder aspectValidatorBuilder)
        {
            _aspectValidator = aspectValidatorBuilder.Build();
        }

        internal bool TryValidate(ServiceDescriptor descriptor, out Type implementationType)
        {
            implementationType = null;

            if (descriptor.ServiceType.GetTypeInfo().IsNonAspect())
            {
                return false;
            }

            if (descriptor.ServiceType.IsGenericTypeDefinition)
            {
                return false;
            }

            implementationType = GetImplementationType(descriptor);

            if (implementationType == null || implementationType == typeof(object))
            {
                return false;
            }

            if (!implementationType.GetTypeInfo().IsClass)
            {
                return false;
            }

            if (descriptor.ServiceType.GetTypeInfo().IsClass)
            {
                if (descriptor.ImplementationType == null)
                {
                    return false;
                }

                if (!implementationType.GetTypeInfo().IsVisible())
                {
                    return false;
                }

                if (!implementationType.GetTypeInfo().CanInherited())
                {
                    return false;
                }
            }

            return _aspectValidator.Validate(descriptor.ServiceType, true) || _aspectValidator.Validate(implementationType, false);
        }


        private Type GetImplementationType(ServiceDescriptor descriptor)
        {
#if NET8_0_OR_GREATER
            if (descriptor.IsKeyedService)
            {
                if (descriptor.KeyedImplementationType != null)
                {
                    return descriptor.KeyedImplementationType;
                }
                else if (descriptor.KeyedImplementationInstance != null)
                {
                    return descriptor.KeyedImplementationInstance.GetType();
                }
                else if (descriptor.KeyedImplementationFactory != null)
                {
                    var typeArguments = descriptor.KeyedImplementationFactory.GetType().GenericTypeArguments;

                    return typeArguments[1];
                }

                return null;
            }
#endif
            if (descriptor.ImplementationType != null)
            {
                return descriptor.ImplementationType;
            }
            else if (descriptor.ImplementationInstance != null)
            {
                return descriptor.ImplementationInstance.GetType();
            }
            else if (descriptor.ImplementationFactory != null)
            {
                var typeArguments = descriptor.ImplementationFactory.GetType().GenericTypeArguments;

                return typeArguments[1];
            }

            return null;
        }
    }
}