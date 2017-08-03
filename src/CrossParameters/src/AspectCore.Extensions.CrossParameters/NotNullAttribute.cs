using System;
using System.Threading.Tasks;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.CrossParameters
{
    public class NotNullAttribute : ParameterInterceptorAttribute
    {
        public string Message { get; set; }

        public NotNullAttribute() { }

        public NotNullAttribute(string message)
        {
            Message = message;
        }

        public override Task Invoke(IParameterDescriptor parameter, ParameterAspectContext context, ParameterAspectDelegate next)
        {
            if (parameter.Value == null)
            {
                throw new ArgumentNullException(parameter.Name, Message);
            }

            return next(parameter, context);
        }
    }
}
