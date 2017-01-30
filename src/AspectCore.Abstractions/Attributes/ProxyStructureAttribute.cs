using System;

namespace AspectCore.Abstractions
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class ProxyStructureAttribute : Attribute
    {
        public ProxyMode ProxyMode { get; }

        public ProxyStructureAttribute(ProxyMode proxyMode)
        {
            ProxyMode = proxyMode;
        }
    }
}
