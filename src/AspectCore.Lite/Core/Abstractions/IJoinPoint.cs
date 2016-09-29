using System;

namespace AspectCore.Lite.Core
{
    public interface IJoinPoint
    {
        Target Target { get; set; }

        Proxy Proxy { get; set; }

        void AddInterceptor(Func<InterceptorDelegate, InterceptorDelegate> @delegate);

        InterceptorDelegate Build();
    }
}
