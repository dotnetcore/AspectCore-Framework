using System;

namespace AspectCore.Injector
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    public class FromContainerAttribute : Attribute
    {
        public FromContainerAttribute()
        {
        }
    }
}