using System;
using System.Runtime.CompilerServices;
using AspectCore.Lite.Common;

namespace AspectCore.Lite.DependencyInjection.Internal
{
    public class ProxyMemorizer : IProxyMemorizer
    {
        private readonly ConditionalWeakTable<object, object> proxyMemorizer =
            new ConditionalWeakTable<object, object>();

        private readonly object lockobj = new object();

        public object GetOrSetProxy(object key, Func<object> proxyFactory)
        {
            ExceptionHelper.ThrowArgumentNull(key, nameof(key));
            ExceptionHelper.ThrowArgumentNull(proxyFactory, nameof(proxyFactory));

            object serviceProxy;
            if (proxyMemorizer.TryGetValue(key, out serviceProxy))
            {
                return serviceProxy;
            }
            lock (lockobj)
            {
                serviceProxy = proxyFactory();
                proxyMemorizer.Add(key, serviceProxy);
            }
            return serviceProxy;
        }

        public bool Remove(object key)
        {
            return proxyMemorizer.Remove(key);
        }
    }
}