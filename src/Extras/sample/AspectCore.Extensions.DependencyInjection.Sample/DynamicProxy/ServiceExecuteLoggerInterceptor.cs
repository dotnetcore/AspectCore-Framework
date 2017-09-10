using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.Injector;
using Microsoft.Extensions.Logging;

namespace AspectCore.Extensions.DependencyInjection.Sample.DynamicProxy
{
    public class ServiceExecuteLoggerInterceptor : InterceptorAttribute
    {
        [FromContainer]
        public ILogger<ServiceExecuteLoggerInterceptor> Logger { get; set; }

        public async override Task Invoke(AspectContext context, AspectDelegate next)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            await next(context);
            stopwatch.Stop();
            Logger?.LogInformation("Executed method {0}.{1}.{2} ({3}) in {4}ms", 
                context.ProxyMethod.DeclaringType.Namespace, 
                context.ProxyMethod.DeclaringType.Name,
                context.ProxyMethod.Name,
                context.ProxyMethod.DeclaringType.Assembly.GetName().Name,
                stopwatch.ElapsedMilliseconds
                );
        }
    }
}
