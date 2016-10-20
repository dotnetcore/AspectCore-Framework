using AspectCore.Lite.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Test.Fakes
{
    public class EmptyInterceptorAttribute: InterceptorAttribute
    {
        public override Task ExecuteAsync(IAspectContext aspectContext , InterceptorDelegate next)
        {
            Console.WriteLine("EmptyInterceptorAttribute Execute");
            return base.ExecuteAsync(aspectContext , next);
        }
    }
}
