using System;
using System.Collections.Generic;
using System.Text;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IExecutableInterceptor
    {
        ExecutionMode Execution { get; set; }
    }
}