using System;

namespace AspectCore.DependencyInjection
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    public class FromServiceContextAttribute : Attribute
    {
        public FromServiceContextAttribute()
        {
        }
    }
}