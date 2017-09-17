using System;
using System.Threading.Tasks;

namespace AspectCore.DynamicProxy.Parameters
{
    public sealed class NotNullAttribute : ParameterInterceptorAttribute
    {
        public override Task Invoke(ParameterAspectContext context, ParameterAspectDelegate next)
        {
            if (context.Parameter.Value == null)
            {
                throw new ArgumentNullException(context.Parameter.Name);
            }
            return next(context);
        }
    }
}