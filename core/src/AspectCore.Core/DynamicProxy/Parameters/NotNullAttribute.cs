using System;
using System.Threading.Tasks;

namespace AspectCore.DynamicProxy.Parameters
{
    public class NotNullAttribute : ParameterInterceptorAttribute
    {
        public string Message { get; set; }

        public override Task Invoke(ParameterAspectContext context, ParameterAspectDelegate next)
        {
            if (context.Parameter.Value == null)
            {
                throw new ArgumentNullException(context.Parameter.Name, Message);
            }
            return next(context);
        }
    }
}