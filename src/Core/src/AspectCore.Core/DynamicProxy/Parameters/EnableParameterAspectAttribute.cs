using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AspectCore.DynamicProxy.Parameters
{
    public class EnableParameterAspectAttribute:InterceptorAttribute
    {
        public override int Order { get; set; } = -999;

        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            return base.Invoke(context, next);
        }
    }
}