using System;

namespace AspectCore.Extensions.IoC
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class KeydAttribute : Attribute
    {
        public object Key { get; }

        public KeydAttribute(object key)
        {
            Key = key;
        }
    }
}