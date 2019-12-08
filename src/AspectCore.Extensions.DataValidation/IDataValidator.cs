using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.DataValidation
{
    [NonAspect]
    public interface IDataValidator
    {
        void Validate(DataValidationContext context);
    }
} 