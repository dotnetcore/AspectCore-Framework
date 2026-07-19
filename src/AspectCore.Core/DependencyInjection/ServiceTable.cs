using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
{
    internal class ServiceTable
    {
        private readonly ConcurrentDictionary<Type, LinkedList<ServiceDefinition>> _linkedServiceDefinitions;
        private readonly ConcurrentDictionary<Type, LinkedList<ServiceDefinition>> _linkedGenericServiceDefinitions;
        private readonly IProxyTypeGenerator _proxyTypeGenerator;
        private readonly ServiceValidator _serviceValidator;

        public ServiceTable(IServiceContext serviceContext)
        {
            if (serviceContext == null)
            {
                throw new ArgumentNullException(nameof(serviceContext));
            }

            var aspectValidatorBuilder = new AspectValidatorBuilder(serviceContext.Configuration);
            _proxyTypeGenerator = CreateProxyTypeGenerator(serviceContext, aspectValidatorBuilder);
            _serviceValidator = new ServiceValidator(aspectValidatorBuilder);
            _linkedServiceDefinitions = new ConcurrentDictionary<Type, LinkedList<ServiceDefinition>>();
            _linkedGenericServiceDefinitions = new ConcurrentDictionary<Type, LinkedList<ServiceDefinition>>();
        }

        private static IProxyTypeGenerator CreateProxyTypeGenerator(IServiceContext serviceContext, IAspectValidatorBuilder aspectValidatorBuilder)
        {
            // 1) 显式实例注册优先
            var explicitGenerator = serviceContext
                .OfType<InstanceServiceDefinition>()
                .FirstOrDefault(x => x.ServiceType == typeof(IProxyTypeGenerator))
                ?.ImplementationInstance as IProxyTypeGenerator;
            if (explicitGenerator != null)
            {
                return explicitGenerator;
            }

            // 2) 按 ProxyEngineOptions 选择（若未配置则保持默认 DynamicProxy）
            var options = serviceContext
                .OfType<InstanceServiceDefinition>()
                .FirstOrDefault(x => x.ServiceType == typeof(ProxyEngineOptions))
                ?.ImplementationInstance as ProxyEngineOptions;

            if (options != null && options.Engine != ProxyEngine.DynamicProxy)
            {
                var registries = serviceContext
                    .OfType<InstanceServiceDefinition>()
                    .Where(x => x.ServiceType == typeof(ISourceGeneratedProxyRegistry))
                    .Select(x => x.ImplementationInstance)
                    .OfType<ISourceGeneratedProxyRegistry>()
                    .ToArray();

                return new SourceGeneratedProxyTypeGenerator(aspectValidatorBuilder, options, registries);
            }

            return new ProxyTypeGenerator(aspectValidatorBuilder);
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
            if (ContainsLinked(serviceType))
            {
                return true;
            }
            if (serviceType.IsConstructedGenericType)
            {
                switch (serviceType.GetGenericTypeDefinition())
                {
                    case Type enumerable when enumerable == typeof(IEnumerable<>):
                    case Type manyEnumerable when manyEnumerable == typeof(IManyEnumerable<>):
                        return ContainsLinked(serviceType.GetTypeInfo().GetGenericArguments()[0]) ? true : true;
                    case Type genericTypeDefinition when _linkedGenericServiceDefinitions.ContainsKey(genericTypeDefinition):
                        return true;
                    default:
                        break;
                }
            }
            return false;
        }

        private bool ContainsLinked(Type serviceType)
        {
            if (_linkedServiceDefinitions.ContainsKey(serviceType))
            {
                return true;
            }
            if (serviceType.IsConstructedGenericType)
            {
                if (_linkedGenericServiceDefinitions.ContainsKey(serviceType.GetGenericTypeDefinition()))
                {
                    return true;
                }
            }
            return false;
        }

        internal ServiceDefinition TryGetService(Type serviceType, object serviceKey = null)
        {
            if (serviceType == null)
            {
                return null;
            }
            if (_linkedServiceDefinitions.TryGetValue(serviceType, out var value))
            {
                if (serviceKey == null)
                {
                    return value.Last.Value;
                }

                ServiceDefinition matched = null;
                foreach (var definition in value)
                {
                    if (Equals(definition.ServiceKey, serviceKey))
                    {
                        matched = definition;
                    }
                }
                return matched;
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
            var elements = FindEnumerableElements(serviceType);
            var enumerableServiceDefinition = new EnumerableServiceDefinition(serviceType, elementType, elements);
            _linkedServiceDefinitions[serviceType] = new LinkedList<ServiceDefinition>(new ServiceDefinition[] { enumerableServiceDefinition });
            return enumerableServiceDefinition;
        }

        private ServiceDefinition FindManyEnumerable(Type serviceType)
        {
            if (_linkedServiceDefinitions.TryGetValue(serviceType, out var value))
            {
                return value.Last.Value;
            }
            var elementType = serviceType.GetTypeInfo().GetGenericArguments()[0];
            var elements = FindEnumerableElements(serviceType);
            var enumerableServiceDefinition = new ManyEnumerableServiceDefinition(serviceType, elementType, elements);
            _linkedServiceDefinitions[serviceType] = new LinkedList<ServiceDefinition>(new ServiceDefinition[] { enumerableServiceDefinition });
            return enumerableServiceDefinition;
        }

        private ServiceDefinition[] FindEnumerableElements(Type serviceType)
        {
            var elementType = serviceType.GetTypeInfo().GetGenericArguments()[0];
            var services = new List<ServiceDefinition>();
            if (_linkedServiceDefinitions.TryGetValue(elementType, out var linkedServices))
            {
                services.AddRange(linkedServices);
            }
            if (elementType.IsConstructedGenericType &&
                _linkedGenericServiceDefinitions.TryGetValue(elementType.GetGenericTypeDefinition(), out var linkedGenericServices))
            {
                services.AddRange(linkedGenericServices.Select(x => MakProxyService(MakGenericService(elementType, x))).Where(x => x != null));
            }
            return services.ToArray();
        }

        private ServiceDefinition FindGenericService(Type serviceType, LinkedList<ServiceDefinition> genericServiceDefinitions)
        {
            if (_linkedServiceDefinitions.TryGetValue(serviceType, out var value))
            {
                return value.Last.Value;
            }
            var service = MakProxyService(MakGenericService(serviceType, genericServiceDefinitions.Last.Value));
            if (service == null)
            {
                return null;
            }
            _linkedServiceDefinitions.TryAdd(serviceType, new LinkedList<ServiceDefinition>(new ServiceDefinition[] { service }));
            return service;
        }

        private ServiceDefinition MakGenericService(Type serviceType, ServiceDefinition service)
        {
            switch (service)
            {
                case InstanceServiceDefinition instanceServiceDefinition:
                    return new InstanceServiceDefinition(serviceType, instanceServiceDefinition.ImplementationInstance, instanceServiceDefinition.ServiceKey);
                case DelegateServiceDefinition delegateServiceDefinition:
                    return new DelegateServiceDefinition(serviceType, delegateServiceDefinition.ImplementationDelegate, delegateServiceDefinition.Lifetime, delegateServiceDefinition.ServiceKey);
                case TypeServiceDefinition typeServiceDefinition:
                    var elementTypes = serviceType.GetTypeInfo().GetGenericArguments();
                    return new TypeServiceDefinition(serviceType, typeServiceDefinition.ImplementationType.MakeGenericType(elementTypes), typeServiceDefinition.Lifetime, typeServiceDefinition.ServiceKey);
                default:
                    return null;
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
