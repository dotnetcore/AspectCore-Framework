using System;

namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public partial class FromServicesAttribute : Attribute
    {
    }
}
