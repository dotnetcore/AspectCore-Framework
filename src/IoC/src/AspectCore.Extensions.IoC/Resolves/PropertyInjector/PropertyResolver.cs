using System;
using AspectCore.Abstractions;
using AspectCore.Extensions.Reflection;

namespace AspectCore.Extensions.IoC.Resolves
{
    internal sealed class PropertyResolver
    {
        private readonly Func<IServiceResolver, object> _propertyFactory;
        private readonly PropertyReflector _reflector;

        public PropertyResolver(Func<IServiceResolver, object> propertyFactory, PropertyReflector reflector)
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