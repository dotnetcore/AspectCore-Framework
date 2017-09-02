using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Configuration;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public sealed class ConfigureInterceptorSelector : IInterceptorSelector
    {
        private readonly IAspectConfiguration _aspectConfiguration;

        public ConfigureInterceptorSelector(IAspectConfiguration aspectConfiguration)
        {
            _aspectConfiguration = aspectConfiguration;
        }

        public IEnumerable<IInterceptor> Select(MethodInfo method)
        {
            return _aspectConfiguration.Interceptors
                .Where(x => x.CanCreated(method))
                .Select(x => x.CreateInstance())
                .OfType<IInterceptor>();
        }
    }
}