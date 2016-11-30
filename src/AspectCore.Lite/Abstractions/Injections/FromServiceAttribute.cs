using System;

namespace AspectCore.Lite.Abstractions
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class FromServiceAttribute : Attribute
    {
    }
}
