using System;
using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface IAspectConfigurator : IEnumerable<Func<MethodInfo, IInterceptor>>, IEnumerable<Func<MethodInfo, bool>>
    {
        void Add(Func<MethodInfo, IInterceptor> configure);

        void Ignore(Func<MethodInfo, bool> configure);
    }
}
