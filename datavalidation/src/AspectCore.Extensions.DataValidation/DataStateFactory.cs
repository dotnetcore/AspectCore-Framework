using System.Linq;

namespace AspectCore.Extensions.DataValidation
{
    public class DataStateFactory : IDataStateFactory
    {
        public IDataState CreateDataState(DataValidationContext dataValidationContext)
        {
            var dataValidationErrors = new DataValidationErrorCollection();
            foreach (var error in dataValidationContext.DataMetaDatas.SelectMany(x => x.Errors))
                dataValidationErrors.Add(error);
            var isValid = dataValidationContext.DataMetaDatas.All(x => x.State != DataValidationState.Invalid);
            return new DataState(isValid, dataValidationErrors);
        }
    }
}