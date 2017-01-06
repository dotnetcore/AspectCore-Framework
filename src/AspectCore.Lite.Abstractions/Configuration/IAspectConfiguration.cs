using System;
using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface IAspectConfiguration
    {
        IConfigurationOption<TOption> GetConfigurationOption<TOption>();
    }
}
