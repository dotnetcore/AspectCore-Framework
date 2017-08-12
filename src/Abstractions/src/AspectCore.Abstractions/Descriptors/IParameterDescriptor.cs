using System;
using System.Reflection;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IParameterDescriptor
    {
        string Name { get; }

        object Value { get; set; }

        Type ParameterType { get; }

        ParameterInfo ParameterInfo { get; }
    }
}
