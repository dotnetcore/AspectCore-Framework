using System;
using System.Reflection;

namespace AspectCore.Abstractions
{
    public interface IParameterDescriptor : ICustomAttributeProvider
    {
        string Name { get; }

        object Value { get; set; }

        Type ParameterType { get; }

        ParameterInfo ParameterInfo { get; }
    }
}
