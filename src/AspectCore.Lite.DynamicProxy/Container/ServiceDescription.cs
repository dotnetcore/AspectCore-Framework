using System;

namespace AspectCore.Lite.DynamicProxy.Container
{
    public sealed class ServiceDescription
    {
        public Type ServiceType { get; }

        public Type ImplementationType { get; }

        public Lifetime Lifetime { get; }

        private ServiceDescription(Type serviceType, Type implementationType, Lifetime lifetime)
        {
            ServiceType = serviceType;
            ImplementationType = implementationType;
            Lifetime = lifetime;
        }

        internal static ServiceDescription Description<TService, TImplementation>(Lifetime lifetime) where TImplementation : TService
        {
            return new ServiceDescription(typeof(TService), typeof(TImplementation), lifetime);
        }
    }
}
