using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Abstractions.Test.Fakes
{
    public class ItemInterceptorAttribute : InterceptorAttribute
    {
        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            context.Data.Add("key", "ItemInterceptor");
            return base.Invoke(context, next);
        }
    }
}
