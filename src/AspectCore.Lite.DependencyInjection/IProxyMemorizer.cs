using System;

namespace AspectCore.Lite.DependencyInjection
{
    public interface IProxyMemorizer
    {
        object GetOrSetProxy(object key, Func<object, Type, object> proxyFactory, Type serviceType);

        bool Remove(object key);
    }
}