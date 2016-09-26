using System;

namespace AspectCore.Lite.Core
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class InjectionAttribute : Attribute
    {
        public Type ServiceType { get; }

        public InjectionAttribute(Type serviceType)
        {
            ServiceType = serviceType;
        }
    }
}
