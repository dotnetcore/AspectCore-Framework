using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Core
{
    public interface IInterceptor
    {
        void Execute(AspectContext aspectContext, InterceptorDelegate next);
    }
}
