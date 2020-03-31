using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Reflection;
using Microsoft.Extensions.Logging;

namespace AspectCore.Extensions.AspNetCore
{
    public class MethodExecuteLoggerInterceptor : AbstractInterceptor
    {
        private static readonly List<string> excepts = new List<string>
        {
            "Microsoft.Extensions.Logging",
            "Microsoft.Extensions.Options",
            "IServiceProvider",
            "IHttpContextAccessor",
            "ITelemetryInitializer",
            "IHostingEnvironment",
            "Autofac.*",
            "Autofac"
        };

        public async override Task Invoke(AspectContext context, AspectDelegate next)
        {
            var serviceType = context.ServiceMethod.DeclaringType;
            if (excepts.Any(x => serviceType.Name.Matches(x)) || excepts.Any(x => serviceType.Namespace.Matches(x)) || context.Implementation is ILogger)
            {
                await context.Invoke(next);
                return;
            }
            var logger = (ILogger<MethodExecuteLoggerInterceptor>)context.ServiceProvider.GetService(typeof(ILogger<MethodExecuteLoggerInterceptor>));
            Stopwatch stopwatch = Stopwatch.StartNew();
            await context.Invoke(next);
            stopwatch.Stop();
            logger?.LogInformation("Executed method {0}.{1}.{2} ({3}) in {4}",
                context.ServiceMethod.DeclaringType.Namespace,
                context.ServiceMethod.DeclaringType.GetReflector().DisplayName,
                context.ServiceMethod.Name,
                context.ServiceMethod.DeclaringType.Assembly.GetName().Name,
                stopwatch.Elapsed
            );
        }
    }
}