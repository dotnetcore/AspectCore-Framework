using System;

namespace AspectCore.Injector
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    public sealed class FromContainerAttribute : Attribute
    { 
        public FromContainerAttribute()
        {
        }
    }
}