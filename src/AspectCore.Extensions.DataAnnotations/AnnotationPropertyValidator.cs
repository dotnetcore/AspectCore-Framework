using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.DataValidation;

namespace AspectCore.Extensions.DataAnnotations
{
    /// <summary>
    /// 基于属性特性校验属性的校验器
    /// </summary>
    [NonAspect]
    public class AnnotationPropertyValidator : IPropertyValidator
    {
        /// <summary>
        /// 校验属性
        /// </summary>
        /// <param name="propertyValidationContext">属性校验上下文</param>
        /// <returns>校验错误集</returns>
        public IEnumerable<DataValidationError> Validate(PropertyValidationContext propertyValidationContext)
        {
            var propertyMetaData = propertyValidationContext.PropertyMetaData;
            foreach (var attribute in propertyMetaData.Attributes)
            {
                if (attribute is ValidationAttribute validation)
                {
                    var validationContext = new ValidationContext(propertyMetaData.Container ?? propertyMetaData.Value, null, null)
                    {
                        MemberName = propertyMetaData.MemberName,
                        DisplayName = propertyMetaData.DisplayName
                    };
                    var result = validation.GetValidationResult(propertyMetaData.Value, validationContext);
                    if (result != ValidationResult.Success)
                    {
                        yield return new DataValidationError(propertyMetaData.MemberName, result.ErrorMessage);
                    }
                }
            }
        }
    }
}