using System;
using System.Collections.Generic;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.IoC.Resolves
{
    internal sealed class EnumerableServiceFactory : IServiceFactory
    {
        public ServiceKey ServiceKey { get; }

        private readonly IServiceFactory[] _servicesFactories;

        public EnumerableServiceFactory(Type itemType, IServiceFactory[] servicesFactories)
        {
            ServiceKey = new ServiceKey(typeof(IEnumerable<>).MakeGenericType(itemType), null);
            _servicesFactories = servicesFactories;
        }

        public object Invoke(IServiceResolver resolver)
        {
            var length = _servicesFactories.Length;
            var results = new object[length];
            for(var i = 0; i < length; i++)
            {
                results[i] = _servicesFactories[i].Invoke(resolver);
            }
            return results;
        }
    }
}