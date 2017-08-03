using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.CrossParameters.Interceptors
{
    public class ParameterInterceptAttribute : InterceptorAttribute
    {
        public override int Order { get; set; } = OrderConstants.ParameterInterception;

        public async override Task Invoke(AspectContext context, AspectDelegate next)
        {
            foreach (var parameter in context.Parameters)
            {
                if (parameter.ParameterType.GetTypeInfo().IsDefined(typeof(NonAspectAttribute)))
                {
                    continue;
                }

                var interceptors = parameter.MatchInterceptors();

                if (!interceptors.Any())
                {
                    continue;
                }

                using (var parameterAspectContext = new ParameterAspectContext(context, parameter))
                {
                    var parameterAspectInvoker = new ParameterAspectInvoker();

                    foreach (var interceptor in interceptors)
                    {
                        parameterAspectInvoker.AddDelegate(interceptor.Invoke);
                    }

                    await parameterAspectInvoker.Invoke(parameter, parameterAspectContext);
                }
            }

            await next(context);
        }
    }
}
