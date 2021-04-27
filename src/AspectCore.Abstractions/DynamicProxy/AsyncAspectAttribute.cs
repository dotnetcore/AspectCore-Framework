using System;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 标识异步拦截
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AsyncAspectAttribute : Attribute
    {
    }
}