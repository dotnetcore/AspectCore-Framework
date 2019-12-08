using System;

namespace AspectCore.DependencyInjection
{
    internal sealed class PropertyInjector : IPropertyInjector
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly PropertyResolver[] _propertyResolvers;

        public PropertyInjector(IServiceProvider serviceProvider, PropertyResolver[] propertyResolvers)
        {
            _serviceProvider = serviceProvider;
            _propertyResolvers = propertyResolvers;
        }

        public void Invoke(object implementation)
        {
            if (implementation == null)
            {
                return;
            }
            var resolverLength = _propertyResolvers.Length;
            if (resolverLength == 0)
            {
                return;
            }
            for (var i = 0; i < resolverLength; i++)
            {
                _propertyResolvers[i].Resolve(_serviceProvider, implementation);
            }
        }
    }
}