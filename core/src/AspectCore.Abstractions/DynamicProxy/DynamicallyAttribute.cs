using System;

namespace AspectCore.DynamicProxy
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class DynamicallyAttribute : Attribute
    {
    }
}
