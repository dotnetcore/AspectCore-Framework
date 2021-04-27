using System;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 标注此特性以表示一个代理类
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class DynamicallyAttribute : Attribute
    {
    }
}
