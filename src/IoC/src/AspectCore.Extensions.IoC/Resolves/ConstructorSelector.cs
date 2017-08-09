using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Abstractions;
using AspectCore.Extensions.Reflection;

namespace AspectCore.Extensions.IoC.Resolves
{
    internal class ConstructorSelector
    {
        private static readonly ConcurrentDictionary<Type, ConstructorResolver> ConstructorResolvers = new ConcurrentDictionary<Type, ConstructorResolver>();

        private readonly HashSet<ServiceKey> _services;
        public ConstructorSelector(IEnumerable<ServiceDefinition> services)
        {
            _services = new HashSet<ServiceKey>(services.Select(x => new ServiceKey(x.ServiceType, x.Key)));
            _services.Add(new ServiceKey(typeof(IServiceResolver), null));
            _services.Add(new ServiceKey(typeof(ConstructorSelector), null));
            _services.Add(new ServiceKey(typeof(IPropertyInjectorFactory), null));
        }

        public ConstructorResolver Select(Type implementationType)
        {
            return ConstructorResolvers.GetOrAdd(implementationType, GetBestConstructor);
        }

        private ConstructorResolver GetBestConstructor(Type implementationType)
        {
            var constructors = implementationType.GetTypeInfo()
               .DeclaredConstructors
               .Where(constructor => constructor.IsPublic)
               .ToArray();
            var length = constructors.Length;
            if (length == 0)
            {
                return null;
            }
            if (length == 1)
            {
                var constructor = constructors[0];
                return TryResolveParameters(constructor, out Func<IServiceResolver, object>[] factories) ? new ConstructorResolver(factories, constructor.GetReflector()) : null;
            }
            Array.Sort(constructors, (a, b) => b.GetParameters().Length.CompareTo(a.GetParameters().Length));
            for (var i = 0; i < length; i++)
            {
                if (TryResolveParameters(constructors[i], out Func<IServiceResolver, object>[] factories))
                {
                    return new ConstructorResolver(factories, constructors[i].GetReflector());
                }
            }
            return null;
        }

        private bool TryResolveParameters(ConstructorInfo constructor, out Func<IServiceResolver, object>[] factories)
        {
            var parameters = constructor.GetParameters();
            factories = new Func<IServiceResolver, object>[parameters.Length];
            if (parameters.Length == 0)
            {
                return true;
            }
            for (var i = 0; i < parameters.Length; i++)
            {
                //get key
                var parameter = parameters[i];
                var serviceType = parameter.ParameterType;
                var key = parameter.GetCustomAttribute<InjectAttribute>()?.Key;
                if (!_services.Contains(new ServiceKey(serviceType, key)))
                {
                    if (!parameter.HasDefaultValue)
                    {
                        return false;
                    }
                    var defaultValue = parameter.DefaultValue;
                    factories[i] = resolver => defaultValue;
                }
                factories[i] = resolver => resolver.Resolve(serviceType, key);
            }
            return true;
        }
    }
}