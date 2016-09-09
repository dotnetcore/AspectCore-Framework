using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Core
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class InjectionAttribute : Attribute
    {
        public Type ServiceType { get; }

        public InjectionAttribute()
        {
        }

        public InjectionAttribute(Type serviceType)
        {
            ServiceType = serviceType;
        }
    }
}
