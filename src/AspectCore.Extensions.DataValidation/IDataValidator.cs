using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.DataValidation
{
    /// <summary>
    /// 数据校验器接口
    /// </summary>
    [NonAspect]
    public interface IDataValidator
    {
        /// <summary>
        /// 校验数据
        /// </summary>
        /// <param name="context">数据校验上下文</param>
        void Validate(DataValidationContext context);
    }
} 