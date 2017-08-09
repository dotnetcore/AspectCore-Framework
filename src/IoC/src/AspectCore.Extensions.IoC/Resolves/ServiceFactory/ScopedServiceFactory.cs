using AspectCore.Abstractions;

namespace AspectCore.Extensions.IoC.Resolves
{
    internal class ScopedServiceFactory : IServiceFactory
    {
        private object _lock = new object();
        private object _instance;
        private IServiceFactory _serviceFactory;

        public ServiceKey ServiceKey { get; }

        public ServiceDefinition ServiceDefinition { get; }

        public ScopedServiceFactory(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
            ServiceKey = serviceFactory.ServiceKey;
            ServiceDefinition = serviceFactory.ServiceDefinition;
        }

        public object Invoke(IServiceResolver serviceResolver)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = _serviceFactory.Invoke(serviceResolver);
                    }
                }
            }
            return _instance;
        }
    }
}