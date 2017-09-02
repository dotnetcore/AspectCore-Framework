using System;

namespace AspectCore.Injector
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    public sealed class InjectAttribute : Attribute
    {
        public object Key { get; }

        public InjectAttribute()
            : this(null)
        {
        }

        public InjectAttribute(object key)
        {
            Key = key;
        }
    }
}