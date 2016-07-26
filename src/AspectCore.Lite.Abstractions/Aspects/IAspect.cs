using System;

namespace AspectCore.Lite.Abstractions.Aspects
{
    public interface IAspect
    {
        Func<IServiceProvider, IAdvice> AdviceFactory { get; set; }
        IPointcut Pointcut { get; set; }
        ITarget Target { get; set; }
        IProxy Proxy { get; set; }
        AspectDelegate CreateDelegate();
    }
}