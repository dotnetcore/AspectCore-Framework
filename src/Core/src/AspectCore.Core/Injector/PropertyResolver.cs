using System;
using AspectCore.Abstractions;
using AspectCore.Extensions.Reflection;

namespace AspectCore.Core.Injector
{
    public sealed class PropertyResolver
    {
        private readonly Func<IServiceProvider, object> _propertyFactory;
        private readonly PropertyReflector _reflector;

        internal PropertyResolver(Func<IServiceProvider, object> propertyFactory, PropertyReflector reflector)
        {  
            _propertyFactory = propertyFactory;
            _reflector = reflector;
        }

        public void Resolve(IServiceProvider provider, object implementation)
        {
            _reflector.SetValue(implementation, _propertyFactory(provider));
        }
    }
}