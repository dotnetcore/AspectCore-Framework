using System;

namespace AspectCore.Lite.Core
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class InjectionAttribute : Attribute
    {
    }
}
