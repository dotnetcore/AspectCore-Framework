using System;
using System.Linq;
using System.Reflection;
using AspectCore.Configuration;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 针对拦截配置进行判断以确定是否需要代理的处理器
    /// </summary>
    public sealed class ConfigureAspectValidationHandler : IAspectValidationHandler
    {
        private readonly IAspectConfiguration _aspectConfiguration;

        /// <summary>
        /// 针对拦截配置进行判断以确定是否需要代理的处理器
        /// </summary>
        /// <param name="aspectConfiguration">拦截配置</param>
        public ConfigureAspectValidationHandler(IAspectConfiguration aspectConfiguration)
        {
            _aspectConfiguration = aspectConfiguration ?? throw new ArgumentNullException(nameof(aspectConfiguration));
        }

        /// <summary>
        /// 排序号，表示处理验证的顺序
        /// </summary>
        public int Order { get; } = 11;

        /// <summary>
        /// 检查是否需要被代理
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="next">后续的验证处理委托</param>
        /// <returns>结果</returns>
        public bool Invoke(AspectValidationContext context, AspectValidationDelegate next)
        {
            if (context.StrictValidation)
            {
                var method = context.Method;
                //配置中不拦截条件适配method,则表示无需代理
                if (_aspectConfiguration.NonAspectPredicates.Any(x => x(method)))
                {
                    return false;
                }
                if (_aspectConfiguration.Interceptors.Where(x => x.Predicates.Length != 0).Any(x => x.CanCreated(method)))
                {
                    return true;
                }
                if (_aspectConfiguration.Interceptors.Where(x => x.Predicates.Length == 0).Any(x => x.CanCreated(method)))
                {
                    return true;
                }
            }
           
            return next(context);
        }
    }
}