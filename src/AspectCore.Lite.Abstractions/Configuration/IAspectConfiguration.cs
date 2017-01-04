using System;
using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface IAspectConfiguration
    {
        void Add(Func<MethodInfo, IInterceptor> configure);

        void Ignore(Func<MethodInfo, bool> configure);

        IConfigurationOption<TOption> GetConfiguration<TOption>();
    }
}
