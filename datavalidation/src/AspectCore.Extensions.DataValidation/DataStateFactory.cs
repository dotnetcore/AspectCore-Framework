using System.Linq;

namespace AspectCore.Extensions.DataValidation
{
    public class DataStateFactory : IDataStateFactory
    {
        public IDataState CreateDataState(DataValidationContext dataValidationContext)
        {
            var dataValidationErrors = new DataValidationErrorCollection();
            foreach (var error in dataValidationContext.DataValidationDescriptors.SelectMany(x => x.Errors))
                dataValidationErrors.Add(error);
            var isValid = dataValidationContext.DataValidationDescriptors.All(x => x.State != DataValidationState.Invalid);
            return new DataState(isValid, dataValidationErrors);
        }
    }
}