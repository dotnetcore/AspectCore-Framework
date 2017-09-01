using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Abstractions;
using AspectCore.Core.DynamicProxy;

namespace AspectCore.Core.Injector
{
    internal class ServiceTable
    {
        private readonly ConcurrentDictionary<Type, LinkedList<ServiceDefinition>> _linkedServiceDefinitions;
        private readonly ConcurrentDictionary<Type, LinkedList<ServiceDefinition>> _linkedGenericServiceDefinitions;
        private readonly IProxyTypeGenerator _proxyTypeGenerator;
        private readonly ServiceValidator _serviceValidator;

        public ServiceTable()
        {
            var aspectValidatorBuilder = new AspectValidatorBuilder(AspectConfigureProvider.Instance);
            _proxyTypeGenerator = new ProxyTypeGenerator(aspectValidatorBuilder);
            _serviceValidator = new ServiceValidator(aspectValidatorBuilder);
            _linkedServiceDefinitions = new ConcurrentDictionary<Type, LinkedList<ServiceDefinition>>();
            _linkedGenericServiceDefinitions = new ConcurrentDictionary<Type, LinkedList<ServiceDefinition>>();
        }

        internal void Populate(IEnumerable<ServiceDefinition> services)
        {
            foreach (var service in services)
            {
                if (service.ServiceType.GetTypeInfo().IsGenericTypeDefinition)
                {
                    var linkedGenericServices = _linkedGenericServiceDefinitions.GetOrAdd(service.ServiceType, _ => new LinkedList<ServiceDefinition>());
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
            var serviceTypeInfo = serviceType.GetTypeInfo();
            if (serviceTypeInfo.IsGenericType)
            {
                switch (serviceTypeInfo.GetGenericTypeDefinition())
                {
                    case Type enumerable when enumerable == typeof(IEnumerable<>):
                        return _linkedServiceDefinitions.ContainsKey(serviceTypeInfo.GetGenericArguments()[0]);
                    case Type genericTypeDefinition when _linkedGenericServiceDefinitions.TryGetValue(genericTypeDefinition, out var genericServiceDefinitions):
                        return _linkedGenericServiceDefinitions.ContainsKey(genericTypeDefinition);
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
            var serviceTypeInfo = serviceType.GetTypeInfo();
            if (serviceTypeInfo.IsGenericType)
            {
                if (serviceTypeInfo.IsGenericTypeDefinition)
                {
                    throw new InvalidOperationException($"GenericTypeDefinition service '{serviceType}' cannot be resolved.");
                }
                switch (serviceTypeInfo.GetGenericTypeDefinition())
                {
                    case Type enumerable when enumerable == typeof(IEnumerable<>):
                        return FindEnumerable(serviceType);
                    case Type genericTypeDefinition when _linkedGenericServiceDefinitions.TryGetValue(genericTypeDefinition, out var genericServiceDefinitions):
                        return FindGenericService(serviceType, genericServiceDefinitions);
                    default:
                        break;
                }
            }
            return null;
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

        private ServiceDefinition MakProxyService(ServiceDefinition service)
        {
            if (service == null)
            {
                return null;
            }
            if (_serviceValidator.TryValidate(service, out Type implType))
            {
                var proxtType = service.ServiceType.GetTypeInfo().IsClass
                    ? _proxyTypeGenerator.CreateClassProxyType(service.ServiceType, implType)
                    : _proxyTypeGenerator.CreateInterfaceProxyType(service.ServiceType, implType);
                return new ProxyServiceDefinition(service, proxtType);
            }
            return service;
        }
    }
}