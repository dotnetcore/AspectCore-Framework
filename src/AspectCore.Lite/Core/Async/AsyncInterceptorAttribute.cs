using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Core.Async
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public abstract class AsyncInterceptorAttribute : OrderableAttribute, IAsyncInterceptor, IInjectable
    {
        public abstract Task ExecuteAsync(AspectContext aspectContext, AsyncInterceptorDelegate next);
    }
}
