using System;
using System.Collections.Generic;
using AspectCore.Core;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public abstract class AspectContext : IDisposable
    {
        public abstract IServiceProvider ServiceProvider { get; }

        public abstract TargetDescriptor Target { get; }

        public abstract IProxyDescriptor Proxy { get; }

        public abstract IParameterCollection Parameters { get; }

        public abstract IParameterDescriptor ReturnParameter { get; }

        public abstract IDictionary<string, object> Items { get; }

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}