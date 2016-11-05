using System;
using System.Threading.Tasks;
using AspectCore.Lite.Abstractions;

namespace AspectCore.Lite.DependencyInjection.Test.Classes
{
    public class CustomeInterceptorAttribute : InterceptorAttribute
    {
        public override async Task ExecuteAsync(IAspectContext aspectContext, InterceptorDelegate next)
        {
            Console.WriteLine("before call");
            await base.ExecuteAsync(aspectContext, next);
            Console.WriteLine("after call");
        }
    }
}