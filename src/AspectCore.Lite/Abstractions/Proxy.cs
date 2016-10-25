using AspectCore.Lite.Internal;
using System;
using System.Linq;
using System.Reflection;

namespace AspectCore.Lite.Abstractions
{
    public sealed class Proxy
    {
        public object Instance { get; }
        public MethodInfo Method { get; }
        public Type ProxyType { get; }

        internal Proxy(object instance, MethodInfo method, Type proxyType)
        {
            ExceptionHelper.ThrowArgumentNull(instance , nameof(instance));
            ExceptionHelper.ThrowArgumentNull(method , nameof(method));
            ExceptionHelper.ThrowArgumentNull(proxyType , nameof(proxyType));

            Instance = instance;
            Method = method;
            ProxyType = proxyType;
        }
    }
}