using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.DataValidation;

namespace AspectCore.Extensions.DataAnnotations
{
    [NonAspect]
    public class AnnotationPropertyValidator : IPropertyValidator
    {
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