using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace AspectCore.Core.Injector
{
    internal sealed class InitialServiceResolver
    {
        private readonly Dictionary<Type, LinkedList<ProxyServiceDefinition>> proxyServiceDefinitions = new Dictionary<Type, LinkedList<ProxyServiceDefinition>>();


    }
}