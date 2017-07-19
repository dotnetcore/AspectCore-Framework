using System;

namespace AspectCore.Abstractions
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    public sealed class NonAspectAttribute : Attribute
    {
    }
}
