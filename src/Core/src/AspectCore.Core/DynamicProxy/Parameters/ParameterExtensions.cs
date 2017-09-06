using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using AspectCore.Extensions.Reflection;
using AspectCore.Utils;

namespace AspectCore.DynamicProxy.Parameters
{
    public static class ParameterExtensions
    {
        private static readonly ConcurrentDictionary<MethodInfo, ParameterReflector[]> _reflectorsCache = new ConcurrentDictionary<MethodInfo, ParameterReflector[]>();
        private static readonly ParameterCollection _emptyParameterCollection = new ParameterCollection(ArrayUtils.Empty<Parameter>());

        public static ParameterCollection GetParameters(this AspectContext aspectContext)
        {
            if (aspectContext == null)
            {
                throw new ArgumentNullException(nameof(aspectContext));
            }
            var reflectors = _reflectorsCache.GetOrAdd(aspectContext.ServiceMethod, m => m.GetParameters().Select(x => x.GetReflector()).ToArray());
            var length = reflectors.Length;
            if (length == 0)
            {
                return _emptyParameterCollection;
            }
            var parameters = new Parameter[length];
            for(var i = 0; i < length; i++)
            {
                parameters[i] = new Parameter(aspectContext.Parameters, i, reflectors[i]);
            }
            return new ParameterCollection(parameters);
        }
    }
}