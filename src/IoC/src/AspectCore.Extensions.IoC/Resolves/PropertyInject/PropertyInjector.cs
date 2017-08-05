using AspectCore.Abstractions;

namespace AspectCore.Extensions.IoC.Resolves
{
    internal sealed class PropertyInjector : IPropertyInjector
    {
        private readonly IServiceResolver _serviceResolver;
        private readonly PropertyResolver[] _propertyResolvers;

        public PropertyInjector(IServiceResolver serviceResolver, PropertyResolver[] propertyResolvers)
        {
            _serviceResolver = serviceResolver;
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
                _propertyResolvers[i].Resolve(_serviceResolver, implementation);
            }
        }
    }
}