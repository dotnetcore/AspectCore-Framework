using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using AspectCore.Extensions.Reflection;
using AspectCore.Injector;
using AspectCore.Utils;

namespace AspectCore.DynamicProxy.Parameters
{
    internal sealed class ParameterInterceptorSelector : IParameterInterceptorSelector
    {
        private static readonly ConcurrentDictionary<ParameterInfo, IParameterInterceptor[]> interceptorCache = new ConcurrentDictionary<ParameterInfo, IParameterInterceptor[]>();
        private readonly IPropertyInjectorFactory _propertyInjectorFactory;

        public ParameterInterceptorSelector(IPropertyInjectorFactory propertyInjectorFactory)
        {
            _propertyInjectorFactory = propertyInjectorFactory ?? throw new ArgumentNullException(nameof(propertyInjectorFactory));
        }

        public IParameterInterceptor[] Select(ParameterInfo parameter)
        {
            var interceptors = interceptorCache.GetOrAdd(parameter, info => info.GetReflector().GetCustomAttributes().OfType<IParameterInterceptor>().ToArray());
            for (var i = 0; i < interceptors.Length; i++)
            {
                var interceptor = interceptors[i];
                if (PropertyInjectionUtils.Required(interceptor))
                {
                    _propertyInjectorFactory.Create(interceptor.GetType()).Invoke(interceptor);
                }
            }
            return interceptors;
        }
    }
}