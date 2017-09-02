using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Reflection;

namespace AspectCore.Injector
{
    internal class ServiceCallSiteResolver
    {
        private readonly ConstructorCallSiteResolver _constructorCallSiteResolver;
        private readonly ConcurrentDictionary<ServiceDefinition, Func<IServiceResolver, object>> _resolvedCallSites;
        public ServiceCallSiteResolver(ServiceTable serviceTable)
        {
            _constructorCallSiteResolver = new ConstructorCallSiteResolver(serviceTable);
            _resolvedCallSites = new ConcurrentDictionary<ServiceDefinition, Func<IServiceResolver, object>>();
        }

        internal Func<IServiceResolver, object> Resolve(ServiceDefinition service)
        {
            return _resolvedCallSites.GetOrAdd(service, d =>
             {
                 switch (d)
                 {
                     case ProxyServiceDefinition proxyServiceDefinition:
                         return ResolveProxyService(proxyServiceDefinition);
                     case InstanceServiceDefinition instanceServiceDefinition:
                         return resolver => instanceServiceDefinition.ImplementationInstance;
                     case DelegateServiceDefinition delegateServiceDefinition:
                         return delegateServiceDefinition.ImplementationDelegate;
                     case TypeServiceDefinition typeServiceDefinition:
                         return ResolveTypeService(typeServiceDefinition);
                     case EnumerableServiceDefintion enumerableServiceDefintion:
                         return ResolveEnumerableService(enumerableServiceDefintion);
                     default:
                         return resolver => null;
                 }
             });
        }

        private Func<IServiceResolver, object> ResolveEnumerableService(EnumerableServiceDefintion enumerableServiceDefintion)
        {
            var elementResolvers = enumerableServiceDefintion.ServiceDefinitions.Select(x => Resolve(x)).ToArray();
            var elementType = enumerableServiceDefintion.ElementType;
            return resolver =>
            {
                var length = elementResolvers.Length;
                var instance = Array.CreateInstance(elementType, length);
                for(var i = 0; i < length; i++)
                {
                    instance.SetValue(elementResolvers[i](resolver), i);
                }
                return instance;
            };
        }

        private Func<IServiceResolver, object> ResolveProxyService(ProxyServiceDefinition proxyServiceDefinition)
        {
            if (proxyServiceDefinition.ServiceType.GetTypeInfo().IsClass)
            {
                return ResolveTypeService((TypeServiceDefinition)proxyServiceDefinition.ServiceDefinition);
            }
            var proxyConstructor = proxyServiceDefinition.ProxyType.GetTypeInfo().GetConstructor(new Type[] { typeof(IAspectActivatorFactory), proxyServiceDefinition.ServiceType });
            var reflector = proxyConstructor.GetReflector();
            var serviceResolver = Resolve(proxyServiceDefinition.ServiceDefinition);
            return resolver => reflector.Invoke(resolver.Resolve<IAspectActivatorFactory>(), serviceResolver(resolver));
        }

        private Func<IServiceResolver, object> ResolveTypeService(TypeServiceDefinition typeServiceDefinition)
        {
            var callSite = _constructorCallSiteResolver.Resolve(typeServiceDefinition.ImplementationType);
            if (callSite == null)
            {
                throw new InvalidOperationException($"Failed to create instance of type '{typeServiceDefinition.ServiceType}'. Possible reason is cannot match the best constructor of type '{typeServiceDefinition.ImplementationType}'.");
            }
            return callSite;
        }
    }
}