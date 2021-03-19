using AspectCore.DynamicProxy;

namespace AspectCore.Configuration
{
    /// <summary>
    /// AspectValidationHandlerCollection的扩展
    /// </summary>
    internal static class AspectValidationHandlerCollectionExtensions
    {
        /// <summary>
        /// 添加默认的验证处理器
        /// </summary>
        /// <param name="aspectValidationHandlers">拦截验证处理器集合</param>
        /// <param name="configuration">配置</param>
        /// <returns>Aspect验证处理器集合</returns>
        internal static AspectValidationHandlerCollection AddDefault(this AspectValidationHandlerCollection aspectValidationHandlers, IAspectConfiguration configuration)
        {
            aspectValidationHandlers.Add(new OverwriteAspectValidationHandler());
            aspectValidationHandlers.Add(new AttributeAspectValidationHandler());
            aspectValidationHandlers.Add(new CacheAspectValidationHandler());
            aspectValidationHandlers.Add(new ConfigureAspectValidationHandler(configuration));
            return aspectValidationHandlers;
        }
    }
}