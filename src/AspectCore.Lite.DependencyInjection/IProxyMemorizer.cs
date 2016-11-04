using System;

namespace AspectCore.Lite.DependencyInjection
{
    public interface IProxyMemorizer
    {
        object GetOrSetProxy(object key, Func<object> proxyFactory);

        bool Remove(object key);
    }
}