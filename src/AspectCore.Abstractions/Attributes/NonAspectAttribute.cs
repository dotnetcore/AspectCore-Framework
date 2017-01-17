using System;

namespace AspectCore.Abstractions
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class NonAspectAttribute : Attribute
    {
    }
}
