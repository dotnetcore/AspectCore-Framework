using System.Collections.Generic;
using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.DataValidation
{
    [NonAspect]
    public interface IPropertyValidator
    {
        IEnumerable<DataValidationError> Validate(PropertyValidationContext propertyValidationContext);
    }
}