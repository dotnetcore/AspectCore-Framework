using System;

namespace AspectCore.Abstractions
{
    [NonAspect]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    public sealed class InjectAttribute : Attribute
    {
        public string Key { get; }

        public InjectAttribute()
            : this(null)
        {
        }

        public InjectAttribute(string key)
        {
            Key = key;
        }
    }
}