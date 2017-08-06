using System.Collections.Generic;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.IoC.Resolves
{
    //internal sealed class ServiceContainerServiceFactory : IServiceFactory
    //{
    //    public const string Key = nameof(ServiceContainerServiceFactory);

    //    public static readonly ServiceKey ServiceContainerKey = new ServiceKey(typeof(IServiceContainer), Key);

    //    public ServiceKey ServiceKey { get; } = ServiceContainerKey;

    //    private readonly IServiceContainer _serviceInstance;

    //    public ServiceContainerServiceFactory(IEnumerable<ServiceDefinition> services)
    //    {
    //        _serviceInstance = new ServiceContainer(services);
    //    }

    //    public object Invoke(IServiceResolver resolver)
    //    {
    //        return _serviceInstance;
    //    }
    //}
}