using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.Utils;

namespace AspectCore.Injector
{
    internal class ServiceTable
    {
        private readonly ConcurrentDictionary<Type, LinkedList<ServiceDefinition>> _linkedServiceDefinitions;
        private readonly ConcurrentDictionary<Type, LinkedList<ServiceDefinition>> _linkedGenericServiceDefinitions;
        private readonly IProxyTypeGenerator _proxyTypeGenerator;
        private readonly ServiceValidator _serviceValidator;

        public ServiceTable(IAspectConfiguration configuration)
        {
            var aspectValidatorBuilder = new AspectValidatorBuilder(configuration);
            _proxyTypeGenerator = new ProxyTypeGenerator(aspectValidatorBuilder);
            _serviceValidator = new ServiceValidator(aspectValidatorBuilder);
            _linkedServiceDefinitions = new ConcurrentDictionary<Type, LinkedList<ServiceDefinition>>();
            _linkedGenericServiceDefinitions = new ConcurrentDictionary<Type, LinkedList<ServiceDefinition>>();
        }

        internal void Populate(IEnumerable<ServiceDefinition> services)
        {
            Func<IEnumerable<ServiceDefinition>, IEnumerable<ServiceDefinition>> filter = input => input.Where(x => !x.IsManyEnumerable());

            foreach (var service in filter(services))
            {
                if (service.ServiceType.GetTypeInfo().ContainsGenericParameters)
                {
                    var linkedGenericServices = _linkedGenericServiceDefinitions.GetOrAdd(service.ServiceType.GetGenericTypeDefinition(), _ => new LinkedList<ServiceDefinition>());
                    linkedGenericServices.Add(service);
                }
                else
                {
                    var linkedServices = _linkedServiceDefinitions.GetOrAdd(service.ServiceType, _ => new LinkedList<ServiceDefinition>());
                    linkedServices.Add(MakProxyService(service));
                }
            }
        }

        internal bool Contains(Type serviceType)
        {
            if (_linkedServiceDefinitions.ContainsKey(serviceType))
            {
                return true;
            }
            if (serviceType.IsConstructedGenericType)
            {
                switch (serviceType.GetGenericTypeDefinition())
                {
                    case Type enumerable when enumerable == typeof(IEnumerable<>):
                        return _linkedServiceDefinitions.ContainsKey(serviceType.GetTypeInfo().GetGenericArguments()[0]);
                    case Type enumerable when enumerable == typeof(IManyEnumerable<>):
                        return _linkedServiceDefinitions.ContainsKey(serviceType.GetTypeInfo().GetGenericArguments()[0]);
                    case Type genericTypeDefinition when _linkedGenericServiceDefinitions.ContainsKey(genericTypeDefinition):
                        return true;
                    default:
                        break;
                }
            }
            return false;
        }

        internal ServiceDefinition TryGetService(Type serviceType)
        {
            if (serviceType == null)
            {
                return null;
            }
            if (_linkedServiceDefinitions.TryGetValue(serviceType, out var value))
            {
                return value.Last.Value;
            }    
            if (serviceType.IsConstructedGenericType)
            {
                switch (serviceType.GetGenericTypeDefinition())
                {
                    case Type enumerable when enumerable == typeof(IEnumerable<>):
                        return FindEnumerable(serviceType);
                    case Type enumerable when enumerable == typeof(IManyEnumerable<>):
                        return FindManyEnumerable(serviceType);
                    case Type genericTypeDefinition when _linkedGenericServiceDefinitions.TryGetValue(genericTypeDefinition, out var genericServiceDefinitions):
                        return FindGenericService(serviceType, genericServiceDefinitions);
                    default:
                        break;
                }
            }
            return null;
        }

        private ServiceDefinition FindEnumerable(Type serviceType)
        {
            if (_linkedServiceDefinitions.TryGetValue(serviceType, out var value))
            {
                return value.Last.Value;
            }
            var elementType = serviceType.GetTypeInfo().GetGenericArguments()[0];
            if (_linkedServiceDefinitions.TryGetValue(elementType, out var services))
            {
                var enumerableServiceDefinition = new EnumerableServiceDefintion(serviceType, elementType, services.ToArray());
                _linkedServiceDefinitions.TryAdd(serviceType, new LinkedList<ServiceDefinition>(new ServiceDefinition[] { enumerableServiceDefinition }));
                return enumerableServiceDefinition;
            }
            return new InstanceServiceDefinition(serviceType, Array.CreateInstance(elementType, 0));
        }

        private ServiceDefinition FindManyEnumerable(Type serviceType)
        {
            if (_linkedServiceDefinitions.TryGetValue(serviceType, out var value))
            {
                return value.Last.Value;
            }
            var elementType = serviceType.GetTypeInfo().GetGenericArguments()[0];
            if (_linkedServiceDefinitions.TryGetValue(elementType, out var services))
            {
                var enumerableServiceDefinition = new ManyEnumerableServiceDefintion(serviceType, elementType, services);
                _linkedServiceDefinitions.TryAdd(serviceType, new LinkedList<ServiceDefinition>(new ServiceDefinition[] { enumerableServiceDefinition }));
                return enumerableServiceDefinition;
            }
            return new InstanceServiceDefinition(serviceType, ActivatorUtils.CreateManyEnumerable(elementType));
        }

        private ServiceDefinition FindGenericService(Type serviceType, LinkedList<ServiceDefinition> genericServiceDefinitions)
        {
            if (_linkedServiceDefinitions.TryGetValue(serviceType, out var value))
            {
                return value.Last.Value;
            }
            var elementTypes = serviceType.GetTypeInfo().GetGenericArguments();

            var service = MakProxyService(MakGenericService());
            _linkedServiceDefinitions.TryAdd(serviceType, new LinkedList<ServiceDefinition>(new ServiceDefinition[] { service }));
            return service;

            ServiceDefinition MakGenericService()
            {
                switch (genericServiceDefinitions.Last.Value)
                {
                    case InstanceServiceDefinition instanceServiceDefinition:
                        return new InstanceServiceDefinition(serviceType, instanceServiceDefinition.ImplementationInstance);
                    case DelegateServiceDefinition delegateServiceDefinition:
                        return new DelegateServiceDefinition(serviceType, delegateServiceDefinition.ImplementationDelegate, delegateServiceDefinition.Lifetime);
                    case TypeServiceDefinition typeServiceDefinition:
                        return new TypeServiceDefinition(serviceType, typeServiceDefinition.ImplementationType.MakeGenericType(elementTypes), typeServiceDefinition.Lifetime);
                    default:
                        return null;
                }
            }
        }

        private ServiceDefinition MakProxyService(ServiceDefinition service)
        {
            if (service == null)
            {
                return null;
            }
            if (_serviceValidator.TryValidate(service, out Type implType))
            {
                var proxyType = service.ServiceType.GetTypeInfo().IsClass
                    ? _proxyTypeGenerator.CreateClassProxyType(service.ServiceType, implType)
                    : _proxyTypeGenerator.CreateInterfaceProxyType(service.ServiceType, implType);
                return new ProxyServiceDefinition(service, proxyType);
            }
            return service;
        }
    }
}