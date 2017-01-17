using AspectCore.Abstractions.Resolution.Generators;
using System;
using System.Reflection;

namespace AspectCore.Abstractions.Resolution
{
    public sealed class ProxyGenerator : IProxyGenerator
    {
        private readonly IAspectValidator aspectValidator;

        public ProxyGenerator(IAspectValidator aspectValidator)
        {
            if (aspectValidator == null)
            {
                throw new ArgumentNullException(nameof(aspectValidator));
            }
            this.aspectValidator = aspectValidator;
        }

        public Type CreateType(Type serviceType, Type implementationType)
        {
            var typeGenerator = new AspectTypeGenerator(serviceType, implementationType, aspectValidator);
            return typeGenerator.CreateType();
        }
    }
}
