using System;
using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface ITargetDescriptor
    {
        //object ImplementationInstance { get; }

        MethodInfo ServiceMethod { get; }

        MethodInfo ImplementationMethod { get; }

        Type ServiceType { get; }

        Type ImplementationType { get; }

        object Invoke(IEnumerable<IParameterDescriptor> parameterDescriptors);
    }
}
