using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace AspectCore.DynamicProxy.Parameters
{
    public static class ParameterExtensions
    {
        private static readonly ConcurrentDictionary<MethodInfo, ParameterInfo[]> _reflectorsCache = new ConcurrentDictionary<MethodInfo, ParameterInfo[]>();
        private static readonly ParameterCollection _emptyParameterCollection = new ParameterCollection(new Parameter[0]);

        public static ParameterCollection GetParameters(this AspectContext aspectContext)
        {
            if (aspectContext == null)
            {
                throw new ArgumentNullException(nameof(aspectContext));
            }
            var reflectors = _reflectorsCache.GetOrAdd(aspectContext.ServiceMethod, m => m.GetParameters());
            var length = reflectors.Length;
            if (length == 0)
            {
                return _emptyParameterCollection;
            }
            var parameters = new Parameter[length];
            for(var i = 0; i < length; i++)
            {
                parameters[i] = new Parameter(aspectContext, i, reflectors[i]);
            }
            return new ParameterCollection(parameters);
        }

        public static Parameter GetReturnParameter(this AspectContext aspectContext)
        {
            if (aspectContext == null)
            {
                throw new ArgumentNullException(nameof(aspectContext));
            }
            return new ReturnParameter(aspectContext, aspectContext.ServiceMethod.ReturnParameter);
        }
    }
}