using System;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.IoC.Resolves
{
    internal sealed class PropertyInjectorFactory : IPropertyInjectorFactory
    {
        private readonly IServiceResolver _serviceResolver;
        private readonly PropertyResolverSelector _propertyResolverSelector;

        public PropertyInjectorFactory(IServiceResolver serviceResolver)
        {
            _serviceResolver = serviceResolver;
            _propertyResolverSelector = PropertyResolverSelector.Default;
        }

        public IPropertyInjector Create(Type implementationType)
        {
            return new PropertyInjector(_serviceResolver, _propertyResolverSelector.SelectPropertyResolver(implementationType));
        }
    }
}