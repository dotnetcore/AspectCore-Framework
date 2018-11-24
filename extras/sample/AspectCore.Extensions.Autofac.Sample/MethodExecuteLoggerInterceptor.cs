using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.Autofac.Sample
{
    public class MethodExecuteLoggerInterceptor : AbstractInterceptor
    {
        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            var stopwatch = Stopwatch.StartNew();
            await next(context);
            stopwatch.Stop();
            Console.WriteLine("Executed method {0}.{1}.{2} ({3}) in {4}ms",
                context.ImplementationMethod.DeclaringType.Namespace,
                context.ImplementationMethod.DeclaringType.Name,
                context.ImplementationMethod.Name,
                context.ImplementationMethod.DeclaringType.Assembly.GetName().Name,
                stopwatch.ElapsedMilliseconds
            );
        }
    }
}
