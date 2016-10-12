using AspectCore.Lite.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Test.Fakes
{
    public class ExecutionTimerInterceptorAttribute: InterceptorAttribute
    {
        public override async Task ExecuteAsync(IAspectContext aspectContext, InterceptorDelegate next)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            await next(aspectContext);
            Console.WriteLine(stopwatch.Elapsed);
        }
    }
}
