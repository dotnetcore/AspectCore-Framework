using AspectCore.Lite.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Test.Fakes
{
    public class InterceptorAppAttribute: InterceptorAttribute
    {
        public override async Task ExecuteAsync(IAspectContext aspectContext, InterceptorDelegate next)
        {
            await next(aspectContext);
            aspectContext.ReturnParameter.Value = "InterceptorApp";
        }
    }
}
