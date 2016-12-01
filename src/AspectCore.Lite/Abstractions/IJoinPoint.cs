using System;

namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface IJoinPoint
    {
        IMethodInvoker MethodInvoker { get; set; }

        void AddInterceptor(Func<InterceptorDelegate, InterceptorDelegate> interceptorDelegate);

        InterceptorDelegate Build();
    }
}
