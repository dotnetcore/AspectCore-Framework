using System;

namespace AspectCore.Lite.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class NonAspectAttribute : Attribute
    {
    }
}
