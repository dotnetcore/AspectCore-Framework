using System.Collections.Generic;

namespace AspectCore.Extensions.DataValidation
{
    public interface IPropertyValidator
    {
        IEnumerable<DataValidationError> Validate(PropertyValidationContext propertyValidationContext);
    }
}