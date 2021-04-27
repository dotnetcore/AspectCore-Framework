using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace AspectCore.DynamicProxy
{
    public sealed class CacheAspectValidationHandler : IAspectValidationHandler
    {
        private readonly ConcurrentDictionary<AspectValidationContext, bool> detectorCache = new ConcurrentDictionary<AspectValidationContext, bool>();

        /// <summary>
        /// 排序号，表示处理验证的顺序
        /// </summary>
        public int Order { get; } = -101;

        /// <summary>
        /// 检查是否需要被代理
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="next">后续的验证处理委托</param>
        /// <returns>结果</returns>
        public bool Invoke(AspectValidationContext context, AspectValidationDelegate next)
        {
            return detectorCache.GetOrAdd(context, tuple => next(context));
        }
    }
}