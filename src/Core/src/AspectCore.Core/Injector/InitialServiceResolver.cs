using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace AspectCore.Core.Injector
{
    internal sealed class InitialServiceResolver
    {
        private readonly ConcurrentDictionary<Type, LinkedList<ProxyServiceDefinition>> proxyServiceDefinitions = new ConcurrentDictionary<Type, LinkedList<ProxyServiceDefinition>>();
    }
}