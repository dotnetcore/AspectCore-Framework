using System;

namespace AspectCore.Lite.Abstractions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method)]
    public sealed class NonAspectAttribute : Attribute
    {
    }
}
