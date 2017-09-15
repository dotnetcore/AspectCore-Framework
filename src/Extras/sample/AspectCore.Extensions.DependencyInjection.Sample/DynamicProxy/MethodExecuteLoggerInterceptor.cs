using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.Injector;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection.Sample.DynamicProxy
{
    public class MethodExecuteLoggerInterceptor : InterceptorAttribute
    {
        public async override Task Invoke(AspectContext context, AspectDelegate next)
        {
            ILogger logger = null;
            //var s = context.ProxyInstance.GetType().Name;
            //if (logger == null)
            //{
            //    logger = context.ServiceProvider.GetService<ILogger<MethodExecuteLoggerInterceptor>>();
            //}
            Stopwatch stopwatch = Stopwatch.StartNew();
            await next(context);
            stopwatch.Stop();
            logger?.LogInformation("Executed method {0}.{1}.{2} ({3}) in {4}ms", 
                context.ServiceMethod.DeclaringType.Namespace, 
                context.ServiceMethod.DeclaringType.Name,
                context.ServiceMethod.Name,
                context.ServiceMethod.DeclaringType.Assembly.GetName().Name,
                stopwatch.ElapsedMilliseconds
                );
        }
    }
}