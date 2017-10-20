using System;

namespace AspectCore.DynamicProxy
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AsyncAspectAttribute : Attribute
    {
    }
}