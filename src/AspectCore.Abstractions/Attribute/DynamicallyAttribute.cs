using System;

namespace AspectCore.Abstractions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class DynamicallyAttribute : Attribute
    {
    }
}
