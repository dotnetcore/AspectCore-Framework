using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspectCore.Lite.Abstractions
{
    public class TargetDescriptor
    {
        public object ImplementationInstance { get; }
        public MethodInfo ServiceMethod { get; }
        public MethodInfo ImplementationMethod { get; }
        public Type ServiceType { get; }
        public Type ImplementationType { get; }

        public TargetDescriptor(object implementationInstance,
            MethodInfo serviceMethod, Type serviceType, MethodInfo implementationMethod, Type implementationType)
        {
            if (implementationInstance == null)
            {
                throw new ArgumentNullException(nameof(implementationInstance));
            }
            if (serviceMethod == null)
            {
                throw new ArgumentNullException(nameof(serviceMethod));
            }
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }
            if (implementationMethod == null)
            {
                throw new ArgumentNullException(nameof(implementationMethod));
            }
            if (implementationType == null)
            {
                throw new ArgumentNullException(nameof(implementationType));
            }

            ServiceMethod = serviceMethod;
            ServiceType = serviceType;
            ImplementationInstance = implementationInstance;
            ImplementationMethod = implementationMethod;
            ImplementationType = implementationType;
        }

        public virtual object Invoke(IEnumerable<ParameterDescriptor> parameterDescriptors)
        {
            try
            {
                var parameters = parameterDescriptors?.Select(descriptor => descriptor.Value).ToArray();
                return ImplementationMethod.Invoke(ImplementationInstance, parameters);
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
    }
}
