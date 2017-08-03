using System.Reflection;
using System.Threading.Tasks;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.CrossParameters.Interceptors
{
    public class MethodInjectAttribute : InterceptorAttribute
    {
        public override int Order { get; set; } = OrderConstants.MethodInjection;

        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            foreach (var parameter in context.Parameters)
            {
                if (parameter.ParameterInfo.IsDefined(typeof(InjectAttribute)))
                {
                    parameter.Value = context.ServiceProvider.GetService(parameter.ParameterType);
                }
            }
            return next(context);
        }
    }
}