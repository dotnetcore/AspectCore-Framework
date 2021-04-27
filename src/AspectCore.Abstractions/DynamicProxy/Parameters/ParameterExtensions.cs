using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace AspectCore.DynamicProxy.Parameters
{
    public static class ParameterExtensions
    {
        private static readonly ConcurrentDictionary<MethodInfo, ParameterInfo[]> _reflectorsCache = new ConcurrentDictionary<MethodInfo, ParameterInfo[]>();
        private static readonly ParameterCollection _emptyParameterCollection = new ParameterCollection(new Parameter[0]);

        /// <summary>
        /// 获取一个ParameterCollection对象，此对象封装被拦截方法的参数上下文信息
        /// </summary>
        /// <param name="aspectContext">拦截上下文</param>
        /// <returns>参数信息</returns>
        public static ParameterCollection GetParameters(this AspectContext aspectContext)
        {
            if (aspectContext == null)
            {
                throw new ArgumentNullException(nameof(aspectContext));
            }
            //获取方法的参数
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

        /// <summary>
        /// 获取被拦截方法的返回值上下文信息
        /// </summary>
        /// <param name="aspectContext">拦截上下文</param>
        /// <returns>返回值上下文信息</returns>
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