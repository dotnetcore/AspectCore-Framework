using System;

namespace AspectCore.Extensions.IoC.Resolves
{
    internal struct ServiceKey
    {
        public Type ServiceType { get; }

        public object Key { get; }

        internal ServiceKey(Type serviceType, object key)
        {
            ServiceType = serviceType;
            Key = key;
        }

        public override bool Equals(object obj)
        {
            if (obj is ServiceKey s)
            {
                return ServiceType == s.ServiceType && Key == s.Key;
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Key != null ? Key.GetHashCode() : 0) * 397) ^ ServiceType.GetHashCode();
            }
        }
    }
}