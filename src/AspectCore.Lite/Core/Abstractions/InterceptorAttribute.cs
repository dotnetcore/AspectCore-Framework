
using AspectCore.Lite.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Core
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public abstract class InterceptorAttribute : OrderableAttribute, IInterceptor, IOrderable, IInjectable
    {
        public abstract void Execute(AspectContext aspectContext, InterceptorDelegate next);
    }
}
