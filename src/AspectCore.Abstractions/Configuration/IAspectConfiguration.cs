using AspectCore.DynamicProxy;
using AspectCore.DependencyInjection;

namespace AspectCore.Configuration
{
    /// <summary>
    /// 拦截配置接口
    /// </summary>
    [NonAspect]
    public interface IAspectConfiguration
    {
        /// <summary>
        /// 验证处理器集合
        /// </summary>
        AspectValidationHandlerCollection ValidationHandlers { get; }

        /// <summary>
        /// 拦截器工厂集合
        /// </summary>
        InterceptorCollection Interceptors { get; }

        /// <summary>
        /// 不拦截条件集合
        /// </summary>
        NonAspectPredicateCollection NonAspectPredicates { get; }

        /// <summary>
        /// 是否抛出了拦截异常
        /// </summary>
        bool ThrowAspectException { get; set; }
    }
}