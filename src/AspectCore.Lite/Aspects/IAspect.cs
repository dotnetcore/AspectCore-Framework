using System;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions.Aspects
{
    public interface IAspect
    {
        IAdvice Advice { get; set; }

        IPointcut Pointcut { get; set; }

    }
}