using System;

namespace AspectCore.Abstractions
{
    [NonAspect]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class FromServicesAttribute : Attribute
    {
    }
}
