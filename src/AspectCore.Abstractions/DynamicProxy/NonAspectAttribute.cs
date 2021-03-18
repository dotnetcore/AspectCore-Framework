using System;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 标注此特性以表示不会被代理
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    public class NonAspectAttribute : Attribute
    {
    }
}
