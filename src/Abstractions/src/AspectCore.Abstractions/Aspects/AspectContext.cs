using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public abstract class AspectContext : IDisposable
    {
        public abstract IDictionary<string, object> AdditionalData { get; }

        public abstract object ReturnValue { get; set; }

        public abstract IServiceProvider ServiceProvider { get; }

        public abstract MethodInfo ServiceMethod { get; }

        public abstract object[] Parameters { get; }

        public abstract MethodInfo ProxyMethod { get; }

        public abstract object ProxyInstance { get; }

        public abstract Task Break();

        public abstract Task Complete();

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}