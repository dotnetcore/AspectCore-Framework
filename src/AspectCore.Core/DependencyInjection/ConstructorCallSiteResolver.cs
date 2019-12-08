using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using AspectCore.Extensions.Reflection;

namespace AspectCore.DependencyInjection
{
    internal sealed class ConstructorCallSiteResolver
    {
        private readonly ConcurrentDictionary<Type, Func<IServiceResolver, object>> compiledCallSites = new ConcurrentDictionary<Type, Func<IServiceResolver, object>>();

        private readonly ServiceTable _serviceTable;

        internal ConstructorCallSiteResolver(ServiceTable serviceTable)
        {
            _serviceTable = serviceTable;
        }

        internal Func<IServiceResolver, object> Resolve(Type implementationType)
        {
            return compiledCallSites.GetOrAdd(implementationType, GetBestCallSite);
        }

        private Func<IServiceResolver, object> GetBestCallSite(Type implementationType)
        {
            var constructors = implementationType.GetTypeInfo()
               .DeclaredConstructors
               .Where(constructor => constructor.IsPublic)
               .ToArray();
            var length = constructors.Length;
            if (length == 0)
            {
                var c = implementationType.GetTypeInfo().GetConstructors();
                return null;
            }
            if (length == 1)
            {
                var constructor = constructors[0];
                return TryResolve(constructor, out Func<IServiceResolver, object> callSite) ? callSite : null;
            }
            Array.Sort(constructors, (a, b) => b.GetParameters().Length.CompareTo(a.GetParameters().Length));
            for (var i = 0; i < length; i++)
            {
                if (TryResolve(constructors[i], out Func<IServiceResolver, object> callSite))
                {
                    return callSite;
                }
            }
            return null;
        }

        private bool TryResolve(ConstructorInfo constructor, out Func<IServiceResolver, object> callSite)
        {
            callSite = null;
            var parameters = constructor.GetParameters();
            var reflector = constructor.GetReflector();
            if (parameters.Length == 0)
            {
                callSite = resolver => reflector.Invoke();
                return true;
            }
            var parameterResolvers = new Func<IServiceResolver, object>[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var serviceType = parameter.ParameterType;
                if (!_serviceTable.Contains(serviceType))
                {
                    if (!parameter.HasDefaultValue)
                    {
                        return false;
                        //throw new InvalidOperationException($"Cannot resolve parameter '{parameter.Name}：{parameter.ParameterType}' for '{constructor}'");
                    }
                    var defaultValue = parameter.DefaultValue;
                    parameterResolvers[i] = resolver => defaultValue;
                }
                else
                {
                    parameterResolvers[i] = resolver => resolver.Resolve(serviceType);
                }
            }
            callSite = resolver =>
            {
                var args = new object[parameterResolvers.Length];
                for (var i = 0; i < args.Length; i++) args[i] = parameterResolvers[i](resolver);
                return reflector.Invoke(args: args);
            };
            return true;
        }
    }
}