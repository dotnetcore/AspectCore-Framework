using System;

namespace AspectCore.Lite.Abstractions
{
    public interface IJoinPoint
    {
        IMethodInvoker ProxyMethodInvoker { get; set; }

        void AddInterceptor(Func<InterceptorDelegate, InterceptorDelegate> @delegate);

        InterceptorDelegate Build();
    }
}
