using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using AspectCore.Abstractions;

namespace AspectCore.Core.Injectors
{
    internal sealed class InitialServiceResolver
    {
        private readonly ConcurrentDictionary<Type, LinkedList<ProxyServiceDefinition>> proxyServiceDefinitions = new ConcurrentDictionary<Type, LinkedList<ProxyServiceDefinition>>();
    }
}