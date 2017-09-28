using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.Injector;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AspectCore.Extensions.AspNetCore
{
    public class MethodExecuteLoggerInterceptor : AbstractInterceptor
    {
        private static readonly HashSet<string> excepts = new HashSet<string>
        {
            "Microsoft.Extensions.Logging",
            "Microsoft.Extensions.Options",
            "IServiceProvider",
            "IHttpContextAccessor",
            "ITelemetryInitializer",
            "IHostingEnvironment"
        };


        public async override Task Invoke(AspectContext context, AspectDelegate next)
        {
            var serviceType = context.ServiceMethod.DeclaringType;
            if (excepts.Contains(serviceType.Name) || excepts.Contains(serviceType.Namespace) ||  context.Implementation is ILogger)
            {
                await context.Invoke(next);
                return;
            }
            //await context.Invoke(next);
            var logger = (ILogger<MethodExecuteLoggerInterceptor>)context.ServiceProvider.GetService(typeof(ILogger<MethodExecuteLoggerInterceptor>));
            Stopwatch stopwatch = Stopwatch.StartNew();
            await context.Invoke(next);
            stopwatch.Stop();
            logger?.LogInformation("Executed method {0}.{1}.{2} ({3}) in {4}",
                context.ServiceMethod.DeclaringType.Namespace,
                context.ServiceMethod.DeclaringType.Name,
                context.ServiceMethod.Name,
                context.ServiceMethod.DeclaringType.Assembly.GetName().Name,
                stopwatch.Elapsed
            );
        }
    }
}