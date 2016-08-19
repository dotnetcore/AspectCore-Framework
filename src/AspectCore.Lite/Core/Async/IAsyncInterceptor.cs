using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Core
{
    public interface IAsyncInterceptor : IOrderable
    {
        Task ExecuteAsync(AspectContext aspectContext, AsyncInterceptorDelegate next);
    }
}