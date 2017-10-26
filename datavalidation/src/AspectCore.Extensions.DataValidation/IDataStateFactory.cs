namespace AspectCore.Extensions.DataValidation
{
    public interface IDataStateFactory
    {
        IDataState CreateDataState(DataValidationContext dataValidationContext);
    }
}