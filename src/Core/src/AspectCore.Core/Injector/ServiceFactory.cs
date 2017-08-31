using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using AspectCore.Abstractions;

namespace AspectCore.Core.Injector
{
    internal sealed class ServiceFactory
    {
        private readonly ConcurrentDictionary<object, Func<IServiceResolver, object>> dictionary = new ConcurrentDictionary<object, Func<IServiceResolver, object>>();

        public Type ServiceType { get; }

        public object Invoke(IServiceResolver resolver, object key)
        {
            return null;
        }
    }
}