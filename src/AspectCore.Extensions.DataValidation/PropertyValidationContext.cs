using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.DataValidation
{
    /// <summary>
    /// 属性校验上下文
    /// </summary>
    public sealed class PropertyValidationContext
    {
        /// <summary>
        /// 用于属性校验的元数据信息
        /// </summary>
        public PropertyMetaData PropertyMetaData { get; }

        /// <summary>
        /// 拦截上下文
        /// </summary>
        public AspectContext AspectContext { get; }

        /// <summary>
        /// 属性校验上下文
        /// </summary>
        /// <param name="propertyMetaData">用于属性校验的元数据信息</param>
        /// <param name="aspectContext">拦截上下文</param>
        public PropertyValidationContext(PropertyMetaData propertyMetaData, AspectContext aspectContext)
        {
            PropertyMetaData = propertyMetaData;
            AspectContext = aspectContext;
        }
    }
}