using System;
using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface ITargetDescriptor
    {
        MethodInfo ServiceMethod { get; }

        Type ServiceType { get; }

        object Invoke(IEnumerable<IParameterDescriptor> parameterDescriptors);
    }
}
