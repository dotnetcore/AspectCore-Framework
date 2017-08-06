using AspectCore.Abstractions;

namespace AspectCore.Extensions.IoC.Resolves
{
    internal sealed class PropertyInjectServiceFactory : IServiceFactory
    {
        private readonly IServiceFactory _serviceFactory;
        public ServiceKey ServiceKey { get; }

        public PropertyInjectServiceFactory(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
            ServiceKey = serviceFactory.ServiceKey;
        }

        public object Invoke(IServiceResolver resolver)
        {
            var result = _serviceFactory.Invoke(resolver);
            if (result != null)
            {
                var injector = resolver.Resolve<IPropertyInjectorFactory>().Create(result.GetType());
                injector.Invoke(result);
            }
            return result;
        }
    }
}