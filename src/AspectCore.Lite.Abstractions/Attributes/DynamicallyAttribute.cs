using System;

namespace AspectCore.Lite.Abstractions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class DynamicallyAttribute : Attribute
    {
    }
}
