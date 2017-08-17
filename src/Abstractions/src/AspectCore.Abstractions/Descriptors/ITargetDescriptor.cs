using System;
using System.Reflection;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface ITargetDescriptor
    {
        MethodInfo ServiceMethod { get; }

        Type ServiceType { get; }
    }
}
