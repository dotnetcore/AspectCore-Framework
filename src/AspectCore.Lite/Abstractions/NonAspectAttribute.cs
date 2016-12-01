using System;

namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method)]
    public sealed class NonAspectAttribute : Attribute
    {
    }
}
