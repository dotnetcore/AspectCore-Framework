using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
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

            if (!_aspectValidator.Validate(descriptor.ServiceType))
            {
                return false;
            }

            implementationType = GetImplementationType(descriptor);

            if (descriptor.ServiceType.GetTypeInfo().IsClass)
            {
                if (descriptor.ImplementationType == null)
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

        private Type GetImplementationType(ServiceDescriptor descriptor)
        {
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