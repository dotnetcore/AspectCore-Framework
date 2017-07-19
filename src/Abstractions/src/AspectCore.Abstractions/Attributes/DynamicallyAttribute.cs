using System;

namespace AspectCore.Abstractions
{
    [NonAspect]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class DynamicallyAttribute : Attribute
    {
    }
}
