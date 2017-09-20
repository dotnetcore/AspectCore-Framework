using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.Autofac.WebSample
{
    public class MethodExecuteLoggerInterceptor : AbstractInterceptor
    {
        public async override Task Invoke(AspectContext context, AspectDelegate next)
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