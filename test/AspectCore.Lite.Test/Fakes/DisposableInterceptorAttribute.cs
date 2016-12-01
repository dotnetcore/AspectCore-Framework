using AspectCore.Lite.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Test.Fakes
{
    public class DisposableInterceptorAttribute : InterceptorAttribute, IDisposable
    {
        public void Dispose()
        {
            Console.WriteLine("Dispose call");
        }
    }
}
