using System;

namespace AspectCore.Lite.Abstractions
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class InjectedAttribute : Attribute
    {
    }
}
