using System;

namespace AspectCore.Lite.Abstractions
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class InjectionAttribute : Attribute
    {
    }
}
