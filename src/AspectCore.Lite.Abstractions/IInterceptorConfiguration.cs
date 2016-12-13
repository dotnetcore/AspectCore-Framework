using System;
using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface IInterceptorConfiguration : IEnumerable<Func<MethodInfo, IInterceptor>>
    {
        void Configure(Func<MethodInfo, IInterceptor> configure);
    }
}
