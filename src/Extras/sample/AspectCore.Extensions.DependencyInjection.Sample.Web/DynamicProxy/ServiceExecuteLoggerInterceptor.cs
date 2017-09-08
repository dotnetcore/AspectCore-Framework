using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.Injector;
using Microsoft.Extensions.Logging;

namespace AspectCore.Extensions.DependencyInjection.Sample.Web.DynamicProxy
{
    public class ServiceExecuteLoggerInterceptor : InterceptorAttribute
    {
        [FromContainer]
        public ILogger<ServiceExecuteLoggerInterceptor> Logger { get; set; }

        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            Logger?.LogInformation("Call Service : {0}--{0}.", context.ServiceMethod.DeclaringType.Name, context.ServiceMethod.Name);
            return next(context);
        }
    }
}
