using System;
using AspectCore.Abstractions;
using AspectCore.Extensions.Reflection;

namespace AspectCore.Core
{
    public sealed class PropertyResolver
    {
        private readonly Func<IServiceResolver, object> _propertyFactory;
        private readonly PropertyReflector _reflector;

        internal PropertyResolver(Func<IServiceResolver, object> propertyFactory, PropertyReflector reflector)
        {  
            _propertyFactory = propertyFactory;
            _reflector = reflector;
        }

        public void Resolve(IServiceResolver resolver, object implementation)
        {
            _reflector.SetValue(implementation, _propertyFactory(resolver));
        }
    }
}