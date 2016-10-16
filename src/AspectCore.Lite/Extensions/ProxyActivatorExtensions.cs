using System;

namespace AspectCore.Lite.Abstractions
{
    public static class ProxyActivatorExtensions
    {
        public static TTarget CreateInterfaceProxy<TTarget>(this IProxyActivator proxyActivator, TTarget instance , params Type[] interfaceTypes)
        {
            return (TTarget)proxyActivator.CreateInterfaceProxy(typeof(TTarget) , instance , interfaceTypes);
        }

        public static TTarget CreateClassProxy<TTarget>(this IProxyActivator proxyActivator , TTarget instance , params Type[] interfaceTypes)
        {
            return (TTarget)proxyActivator.CreateClassProxy(typeof(TTarget) , instance , interfaceTypes);
        }
    }
}
