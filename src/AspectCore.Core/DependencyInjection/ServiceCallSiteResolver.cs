using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Reflection;
using AspectCore.Utils;

namespace AspectCore.DependencyInjection
{
    internal class ServiceCallSiteResolver
    {
        private readonly ConstructorCallSiteResolver _constructorCallSiteResolver;
        private readonly ConcurrentDictionary<ServiceDefinition, Func<ServiceResolver, object>> _resolvedCallSites;

        public ServiceCallSiteResolver(ServiceTable serviceTable)
        {
            _constructorCallSiteResolver = new ConstructorCallSiteResolver(serviceTable);
            _resolvedCallSites = new ConcurrentDictionary<ServiceDefinition, Func<ServiceResolver, object>>();
        }

        internal Func<ServiceResolver, object> Resolve(ServiceDefinition service)
        {
            return _resolvedCallSites.GetOrAdd(service, ResolveCallback);
        }

        private Func<ServiceResolver, object> ResolveCallback(ServiceDefinition service)
        {
            var callSite = ResolveInternal(service);
            if (!service.RequiredResolveCallback())
            {
                return callSite;
            }
            
            return resolver =>
            {
                var instance = callSite(resolver);
                var callbacks = resolver.ServiceResolveCallbacks;
                for (var i = 0; i < callbacks.Length; i++)
                {
                    instance = callbacks[i].Invoke(resolver, instance, service);
                }
                return instance;
            };
        }

        private Func<ServiceResolver, object> ResolveInternal(ServiceDefinition service)
        {
            switch (service)
            {
                case ProxyServiceDefinition proxyServiceDefinition:
                    return ResolveProxyService(proxyServiceDefinition);
                case InstanceServiceDefinition instanceServiceDefinition:
                    return resolver => instanceServiceDefinition.ImplementationInstance;
                case DelegateServiceDefinition delegateServiceDefinition:
                    return delegateServiceDefinition.ImplementationDelegate;
                case TypeServiceDefinition typeServiceDefinition:
                    return ResolveTypeService(typeServiceDefinition);
                case ManyEnumerableServiceDefintion manyEnumerableServiceDefinition:
                    return ResolveManyEnumerableService(manyEnumerableServiceDefinition);
                case EnumerableServiceDefintion enumerableServiceDefinition:
                    return ResolveEnumerableService(enumerableServiceDefinition);
                default:
                    return resolver => null;
            }

            ;
        }

        private Func<ServiceResolver, object> ResolveManyEnumerableService(ManyEnumerableServiceDefintion manyEnumerableServiceDefintion)
        {
            var elementDefinitions = manyEnumerableServiceDefintion.ServiceDefinitions.ToArray();
            var elementType = manyEnumerableServiceDefintion.ElementType;
            return resolver =>
            {
                var length = elementDefinitions.Length;
                var instance = Array.CreateInstance(elementType, length);
                for (var i = 0; i < length; i++)
                {
                    instance.SetValue(resolver.ResolveDefinition(elementDefinitions[i]), i);
                }

                return ActivatorUtils.CreateManyEnumerable(elementType, instance);
            };
        }

        private Func<ServiceResolver, object> ResolveEnumerableService(EnumerableServiceDefintion enumerableServiceDefintion)
        {
            var elementDefinitions = enumerableServiceDefintion.ServiceDefinitions.ToArray();
            var elementType = enumerableServiceDefintion.ElementType;
            return resolver =>
            {
                var length = elementDefinitions.Length;
                var instance = Array.CreateInstance(elementType, length);
                for (var i = 0; i < length; i++)
                {
                    var element = resolver.ResolveDefinition(elementDefinitions[i]);
                    instance.SetValue(element, i);
                }

                return instance;
            };
        }

        private Func<ServiceResolver, object> ResolveProxyService(ProxyServiceDefinition proxyServiceDefinition)
        {
            if (proxyServiceDefinition.ServiceType.GetTypeInfo().IsClass)
            {
                return ResolveTypeService(proxyServiceDefinition.ClassProxyServiceDefinition);
            }

            var proxyConstructor = proxyServiceDefinition.ProxyType.GetTypeInfo().GetConstructor(new Type[] {typeof(IAspectActivatorFactory), proxyServiceDefinition.ServiceType});
            var reflector = proxyConstructor.GetReflector();
            var serviceResolver = Resolve(proxyServiceDefinition.ServiceDefinition);
            return resolver => reflector.Invoke(resolver.ResolveRequired<IAspectActivatorFactory>(), serviceResolver(resolver));
        }

        private Func<ServiceResolver, object> ResolveTypeService(TypeServiceDefinition typeServiceDefinition)
        {
            var callSite = _constructorCallSiteResolver.Resolve(typeServiceDefinition.ImplementationType);
            if (callSite == null)
            {
                throw new InvalidOperationException(
                    $"Failed to create instance of type '{typeServiceDefinition.ServiceType}'. Possible reason is cannot match the best constructor of type '{typeServiceDefinition.ImplementationType}'.");
            }

            return callSite;
        }
    }
}