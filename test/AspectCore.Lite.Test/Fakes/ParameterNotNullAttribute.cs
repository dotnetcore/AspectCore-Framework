using AspectCore.Lite.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Test.Fakes
{
    public class ParameterNotNullAttribute : InterceptorAttribute
    {
        public override Task ExecuteAsync(IAspectContext aspectContext, InterceptorDelegate next)
        {
            foreach (var parameter in aspectContext.Parameters)
            {
                if (parameter.Value == null)
                {
                    throw new ArgumentNullException(parameter.Name);
                }
            }
            return base.ExecuteAsync(aspectContext, next);
        }
    }
}
