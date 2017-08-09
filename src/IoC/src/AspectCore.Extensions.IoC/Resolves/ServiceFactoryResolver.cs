using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using AspectCore.Abstractions;
using System.Linq;
using System.Reflection;

namespace AspectCore.Extensions.IoC.Resolves
{
    internal sealed class ServiceFactoryResolver : IDisposable
    {
        private readonly object _root;
        private readonly ConcurrentDictionary<ServiceKey, IList<IServiceFactory>> _serviceFactories;
        //private readonly 

        public ServiceFactoryResolver(IEnumerable<ServiceDefinition> services)
        {
            _serviceFactories = new ConcurrentDictionary<ServiceKey, IList<IServiceFactory>>();
            Populate(services);
        }

        private void Populate(IEnumerable<ServiceDefinition> serviceDefinitions)
        {
            foreach (var services in serviceDefinitions.GroupBy(x =>new ServiceKey( x.ServiceType,x.Key)))
            {
                var serviceFactoryList = new List<IServiceFactory>();
                foreach (var service in services)
                {
                    var factory = service.CreateServiceFactory();
                    //if (service.ServiceType.GetTypeInfo().IsInterface)
                    //{
                    //    if (services.Count(x => x.ServiceType == service.ServiceType) ==1)
                    //    {
                    //        factory = new ProxyServiceFactory(factory, false);
                    //    }
                    //    else
                    //    {
                    //        factory = new ProxyServiceFactory(factory, true);
                    //    }
                    //}
                    //else
                    //{
                    //    factory = new ProxyServiceFactory(factory, true);
                    //}
                    if (service.Lifetime != Lifetime.Transient)
                    {
                        factory = new ScopedServiceFactory(factory);
                    }
                    serviceFactoryList.Add(factory);
                }
                _serviceFactories.TryAdd(services.Key, serviceFactoryList);
            }
            _serviceFactories.TryAdd(ServiceResolverServiceFactory.ServiceResolverKey, new List<IServiceFactory>() { new ServiceResolverServiceFactory() });
            _serviceFactories.TryAdd(new ServiceKey(typeof(ConstructorSelector), null), new List<IServiceFactory>() { new InstanceServiceFactory(new InstanceServiceDefinition(typeof(ConstructorSelector), new ConstructorSelector(serviceDefinitions), null)) });
            _serviceFactories.TryAdd(new ServiceKey(typeof(IPropertyInjectorFactory), null), new List<IServiceFactory>() { new DelegateServiceFactory(new DelegateServiceDefinition(typeof(ConstructorSelector), resolver => new PropertyInjectorFactory(resolver), Lifetime.Scoped, null)) });
        }

        public ServiceFactoryResolver(ServiceFactoryResolver root)
        {
            _root = root;
            _serviceFactories = root._serviceFactories;
        }

        public IServiceFactory Resolve(ServiceKey key)
        {
            if(_serviceFactories.TryGetValue(key,out IList<IServiceFactory> list))
            {
                return list[list.Count - 1];
            }
            var serviceTypeInfo = key.ServiceType.GetTypeInfo();
            if (!serviceTypeInfo.IsGenericType)
            {
                return null;
            }
            if (serviceTypeInfo.IsGenericTypeDefinition)
            {
                return null;
            }
            if (serviceTypeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                if (key.Key != null)
                {
                    return null;
                }
                var keys = _serviceFactories.Keys.Where(x => x.ServiceType == key.ServiceType).ToArray();
                _serviceFactories.GetOrAdd(key, _ =>
                 {
                     var allServiceFactory = new List<IServiceFactory>();
                     foreach (var serviceKey in keys)
                     {
                         if (_serviceFactories.TryGetValue(serviceKey, out IList<IServiceFactory> _list))
                         {
                             allServiceFactory.AddRange(_list);
                         }
                     }
                     return new List<IServiceFactory>() { new EnumerableServiceFactory(serviceTypeInfo.GetGenericArguments()[0], allServiceFactory.ToArray()) };
                 });
            }
            return null;
        }

        public ServiceFactoryResolver CreateScope()
        {
            return new ServiceFactoryResolver(this);
        }

        public void Dispose()
        {
            if (_root != null)
            {

            }
        }
    }
}