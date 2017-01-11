using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions.Resolution.Test.Fakes
{
    public class IncrementAttribute : InterceptorAttribute
    {
        public async override Task Invoke(IAspectContext context, AspectDelegate next)
        {
            await next(context);
            if (context.ReturnParameter.ParameterType == typeof(int))
            {
                int value;
                if(int.TryParse(context.ReturnParameter.Value.ToString(), out value))
                {
                    value = value + 1;
                    context.ReturnParameter.Value = value;
                }
            }
        }
    }
}
