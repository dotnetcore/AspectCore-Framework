using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AspectCore.Abstractions;
using AspectCore.Core.DynamicProxy;
using AspectCore.Core.Utils;

namespace AspectCore.Core.Injector
{
    internal class ServiceResolver : IServiceResolver
    {
        private readonly ConcurrentDictionary<ServiceDefinition, object> _resolvedScopedServcies;
        private readonly ConcurrentDictionary<ServiceDefinition, object> _resolvedSingletonServcies;
        private readonly IEnumerable<ServiceDefinition> _initialServiceDefinitions;
        private readonly Dictionary<Type, LinkedList<ServiceDefinition>> _linkedServiceDefinitions;
        private readonly ConcurrentDictionary<ServiceDefinition, Func<IServiceResolver, object>> _resolvedFactories;
        private readonly Dictionary<Type, LinkedList<ServiceDefinition>> _linkedGenericServiceDefinitions;

        private readonly IServiceResolver _root;

        public ServiceResolver(IEnumerable<ServiceDefinition> serviceDefinitions)
        {
            _initialServiceDefinitions = serviceDefinitions;
        }

        private void Populate()
        {
            var aspectValidatorBuilder = new AspectValidatorBuilder(AspectConfigureProvider.Instance);
            var proxyGengrator = new ProxyTypeGenerator(aspectValidatorBuilder);
            var aspectValidator = aspectValidatorBuilder.Build();
            foreach (var service in _initialServiceDefinitions)
            {
                LinkedList<ServiceDefinition> linkedList = new LinkedList<ServiceDefinition>();
                if (ServiceValidateUtils.TryValidate(service, aspectValidator, out Type implType))
                {
                    if (service.ServiceType.GetTypeInfo().IsClass)
                    {
                        var proxy = new ProxyServiceDefinition(service, proxyGengrator.CreateClassProxyType(service.ServiceType, implType));
                    }
                    else
                    {
                        var proxy = new ProxyServiceDefinition(service, proxyGengrator.CreateClassProxyType(service.ServiceType, implType));
                    }
                }
                else
                {

                }
                _linkedServiceDefinitions.Add(service.ServiceType, linkedList);
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public object GetService(Type serviceType)
        {
            return Resolve(serviceType);
        }

        public object Resolve(Type serviceType)
        {
            if (_linkedServiceDefinitions.TryGetValue(serviceType, out var value))
            {
                var definition = value.Last.Value;
                switch (definition.Lifetime)
                {
                    case Lifetime.Singleton:
                        return _resolvedSingletonServcies.GetOrAdd(definition, ResolveSingleton);
                    case Lifetime.Scoped:
                        return _resolvedScopedServcies.GetOrAdd(definition, key => null);
                    default:
                        return null;
                }
            }
            var serviceTypeInfo = serviceType.GetTypeInfo();
            if (serviceTypeInfo.IsGenericType)
            {
                switch (serviceTypeInfo.GetGenericTypeDefinition())
                {
                    case Type enumerable when enumerable == typeof(IEnumerable<>):
                        return ResolveEnumerable(serviceType);
                    case Type genericTypeDefinition when _linkedGenericServiceDefinitions.TryGetValue(genericTypeDefinition, out var genericServiceDefinitions):
                        return ResolveGenericService(serviceType, genericServiceDefinitions);
                    default:
                        break;
                }
            }
            return null;
        }


        private object ResolveSingleton(ServiceDefinition serviceDefinition)
        {
            if(serviceDefinition is InstanceServiceDefinition ins)
            {
                return ins.ImplementationInstance;
            }
            else if(serviceDefinition is DelegateServiceDefinition dele)
            {
                return dele.ImplementationDelegate(_root ?? this);
            }
            return null;
        }

        private IEnumerable<object> ResolveEnumerable(Type serviceType)
        {
            if (_linkedServiceDefinitions.TryGetValue(serviceType, out var linkedList))
            {
                foreach(var item in linkedList)
                {
                    yield return null;
                }
            }
        }

        private object ResolveGenericService(Type serviceType, LinkedList<ServiceDefinition> genericServiceDefinitions)
        {
            return null;
        }

    }
}