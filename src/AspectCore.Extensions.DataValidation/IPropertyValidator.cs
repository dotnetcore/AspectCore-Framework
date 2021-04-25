using System.Collections.Generic;
using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.DataValidation
{
    /// <summary>
    /// 属性校验器接口
    /// </summary>
    [NonAspect]
    public interface IPropertyValidator
    {
        /// <summary>
        /// 校验属性
        /// </summary>
        /// <param name="propertyValidationContext">属性校验上下文</param>
        /// <returns>校验错误集</returns>
        IEnumerable<DataValidationError> Validate(PropertyValidationContext propertyValidationContext);
    }
}