using System;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 被此特性标注的对象将从容器中获取
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    public class FromServiceContextAttribute : Attribute
    {
        public FromServiceContextAttribute()
        {
        }
    }
}