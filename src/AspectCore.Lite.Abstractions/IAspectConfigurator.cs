using System;
using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface IAspectConfigurator : IEnumerable<Func<MethodInfo, IInterceptor>>
    {
        void Configure(Func<MethodInfo, IInterceptor> configure);
    }
}
