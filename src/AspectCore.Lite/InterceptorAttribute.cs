
using AspectCore.Lite.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Core
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public abstract class InterceptorAttribute : Attribute, IInterceptor, IOrderable
    {
        public int Order { get; set; }

        public int CompareTo(IOrderable other)
        {
            if (other == null) return 1;
            return Order.CompareTo(other.Order);
        }

        public abstract void Execute(AspectContext aspectContext, InterceptorDelegate next);
    }
}
