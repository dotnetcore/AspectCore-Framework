using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions.Aspects
{
    public interface IAdvice
    {
        Task ExecuteAsync(AspectContext context, AspectDelegate next);
    }
}
