using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Abstractions;

namespace AspectCore.Core.DynamicProxy
{
    [NonAspect]
    public sealed class ConfigureInterceptorSelector : IInterceptorSelector
    {
        private readonly IAspectConfiguration _aspectConfiguration;
        private readonly IServiceProvider _serviceProvider;

        public ConfigureInterceptorSelector(IAspectConfiguration aspectConfiguration, IServiceProvider serviceProvider)
        {
            _aspectConfiguration = aspectConfiguration;
            _serviceProvider = serviceProvider;
        }

        public IEnumerable<IInterceptor> Select(MethodInfo method)
        {
            return _aspectConfiguration.Interceptors
                .Where(x => x.CanCreated(method))
                .Select(x => x.CreateInstance(_serviceProvider))
                .OfType<IInterceptor>();
        }
    }
}