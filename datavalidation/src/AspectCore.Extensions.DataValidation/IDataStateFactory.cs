using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.DataValidation
{
    [NonAspect]
    public interface IDataStateFactory
    {
        IDataState CreateDataState(DataValidationContext dataValidationContext);
    }
}