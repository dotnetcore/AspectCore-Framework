using AspectCore.Lite.Abstractions.Resolution.Generators;
using System;
using System.Reflection;

namespace AspectCore.Lite.Abstractions.Resolution
{
    public sealed class ProxyGenerator : IProxyGenerator
    {
        private readonly IAspectValidator aspectValidator;

        public ProxyGenerator(IAspectValidator aspectValidator)
        {
            this.aspectValidator = aspectValidator;
        }

        public Type CreateType(Type serviceType, Type implementationType)
        {
            var typeGenerator = new AspectTypeGenerator(serviceType, implementationType, aspectValidator);
            return typeGenerator.CreateType();
        }
    }
}
