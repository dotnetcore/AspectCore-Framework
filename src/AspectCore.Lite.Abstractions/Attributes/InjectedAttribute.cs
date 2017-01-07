using System;

namespace AspectCore.Lite.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class InjectedAttribute : Attribute
    {
    }
}
