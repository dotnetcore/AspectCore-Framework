using System.Reflection;
using System.Linq;
using AspectCore.Abstractions;
using System;
using AspectCore.Extensions.Reflection;

namespace AspectCore.Extensions.IoC.Resolves
{
    internal class ProxyServiceFactory : IServiceFactory
    {
        private readonly bool _isCreateClassProxy;
        private readonly object _lock = new object();

        public IServiceFactory DynamicallyServiceFactory { get; set; }

        public IServiceFactory ServiceFactory { get; }

        public ServiceKey ServiceKey { get; }

        public ServiceDefinition ServiceDefinition { get; }

        public ProxyServiceFactory(IServiceFactory serviceFactory, bool isCreateClassProxy)
        {
            ServiceFactory = serviceFactory;
            ServiceKey = serviceFactory.ServiceKey;
            ServiceDefinition = serviceFactory.ServiceDefinition;
            _isCreateClassProxy = isCreateClassProxy;
        }

        public object Invoke(IServiceResolver serviceResolver)
        {
            if (DynamicallyServiceFactory == null)
            {
                lock (_lock)
                {
                    if (DynamicallyServiceFactory == null)
                    {
                        DynamicallyServiceFactory = ResolveServiceFactory(serviceResolver);
                    }
                }
            }
            return DynamicallyServiceFactory.Invoke(serviceResolver);
        }

        private IServiceFactory ResolveServiceFactory(IServiceResolver serviceResolver)
        {
            var aspectValidator = serviceResolver.Resolve<IAspectValidatorBuilder>().Build();
            if (TryValidate(ServiceDefinition, aspectValidator, out Type implementationType))
            {
                var proxyGenerator = serviceResolver.Resolve<IProxyGenerator>();
                var proxyType = _isCreateClassProxy
                    ? proxyGenerator.CreateClassProxyType(ServiceKey.ServiceType, implementationType)
                    : proxyGenerator.CreateInterfaceProxyType(ServiceKey.ServiceType, implementationType);
                return new PropertyInjectServiceFactory(
                    new TypeServiceFactory(
                        new TypeServiceDefinition(
                            ServiceDefinition.ServiceType,
                            proxyType,
                            ServiceDefinition.Lifetime,
                            ServiceDefinition.Key)));
            }
            else
            {
                return ServiceFactory;
            }
        }

        private static bool TryValidate(ServiceDefinition definition, IAspectValidator aspectValidator, out Type implementationType)
        {
            implementationType = null;

            if (!definition.ServiceType.GetTypeInfo().DeclaredMethods.Any(x => aspectValidator.Validate(x)))
            {
                return false;
            }

            implementationType = definition.GetImplementationType();

            if (definition.ServiceType.GetTypeInfo().IsClass)
            {
                if (implementationType == null)
                {
                    return false;
                }
                if (!CanInherited(implementationType.GetTypeInfo()))
                {
                    return false;
                }
            }

            if (implementationType == null)
            {
                return false;
            }

            return !implementationType.GetReflector().IsDefined(typeof(DynamicallyAttribute));

            bool CanInherited(TypeInfo typeInfo)
            {
                if (typeInfo == null)
                {
                    throw new ArgumentNullException(nameof(typeInfo));
                }

                if (!typeInfo.IsClass || typeInfo.IsSealed)
                {
                    return false;
                }

                if (typeInfo.GetReflector().IsDefined<NonAspectAttribute>() || typeInfo.GetReflector().IsDefined<DynamicallyAttribute>())
                {
                    return false;
                }

                if (typeInfo.IsNested)
                {
                    return typeInfo.IsNestedPublic && typeInfo.DeclaringType.GetTypeInfo().IsPublic;
                }
                else
                {
                    return typeInfo.IsPublic;
                }
            }
        }
    }
}