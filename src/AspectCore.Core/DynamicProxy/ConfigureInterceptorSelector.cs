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
        private readonly IServiceProvider _serviceProvider;

        public ConfigureInterceptorSelector(IAspectConfiguration aspectConfiguration, IServiceProvider serviceProvider)
        {
            _aspectConfiguration = aspectConfiguration ?? throw new ArgumentNullException(nameof(aspectConfiguration));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public IEnumerable<IInterceptor> Select(MethodInfo method)
        {
            //todo fix nonaspect
            foreach (var interceptorFactory in _aspectConfiguration.Interceptors)
            {
                if (interceptorFactory.Predicates.Length != 0)
                {
                    if (interceptorFactory.CanCreated(method))
                        yield return interceptorFactory.CreateInstance(_serviceProvider);
                }
                else
                {
                    if (!_aspectConfiguration.NonAspectPredicates.Any(x => x(method)))
                        yield return interceptorFactory.CreateInstance(_serviceProvider);
                }
            }
        }
    }
}