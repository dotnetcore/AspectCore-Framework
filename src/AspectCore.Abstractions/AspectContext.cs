using System;
using System.Collections.Generic;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public class AspectContext : IDisposable
    {
        public virtual IServiceProvider ServiceProvider { get; }

        public virtual TargetDescriptor Target { get; }

        public virtual ProxyDescriptor Proxy { get; }

        public virtual ParameterCollection Parameters { get; }

        public virtual ParameterDescriptor ReturnParameter { get; }

        public virtual IDictionary<string, object> Items { get; }

        protected virtual void Dispose(bool disposing)
        { 
        }

        public void Dispose()
        {   
            Dispose(true);
        }
    }
}