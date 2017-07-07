using System;
using System.Collections.Generic;
using System.Text;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IExecutableInterceptorValidator
    {
        bool CanExecute(AspectContext context, IExecutableInterceptor interceptor);
    }
}
