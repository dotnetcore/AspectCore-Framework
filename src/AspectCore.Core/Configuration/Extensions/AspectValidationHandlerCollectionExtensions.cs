using AspectCore.DynamicProxy;

namespace AspectCore.Configuration
{
    internal static class AspectValidationHandlerCollectionExtensions
    {
        /// <summary>
        /// 添加默认的验证检查处理器
        /// </summary>
        /// <param name="aspectValidationHandlers">拦截验证处理器集合</param>
        /// <param name="configuration">配置</param>
        /// <returns>AspectValidationHandlerCollection</returns>
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