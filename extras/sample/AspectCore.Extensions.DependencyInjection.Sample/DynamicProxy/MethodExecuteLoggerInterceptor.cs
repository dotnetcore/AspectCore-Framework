using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.Injector;
using Microsoft.Extensions.Logging;

namespace AspectCore.Extensions.DependencyInjection.Sample.DynamicProxy
{
    public class MethodExecuteLoggerInterceptor : AbstractInterceptor
    {
        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            await next(context);
            stopwatch.Stop();
            Console.WriteLine("Executed method {0}.{1}.{2} ({3}) in {4}ms",
                context.ServiceMethod.DeclaringType.Namespace,
                context.ServiceMethod.DeclaringType.Name,
                context.ServiceMethod.Name,
                context.ServiceMethod.DeclaringType.Assembly.GetName().Name,
                stopwatch.ElapsedMilliseconds
            );
        }
    }
}