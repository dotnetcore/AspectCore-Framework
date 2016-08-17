using AspectCore.Lite;
using AspectCore.Lite.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Core
{
    public interface IAspectFactory
    {
        Aspect Create(IInterceptor interceptor , IPointcut pointcut);

        Aspect Create(IAsyncInterceptor asyncInterceptor, IPointcut pointcut);

        Aspect Create(Type interceptorType , IPointcut pointcut);
    }
}
