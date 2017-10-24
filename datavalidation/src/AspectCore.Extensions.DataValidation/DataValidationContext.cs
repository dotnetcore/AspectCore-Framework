using System.Collections.Generic;
using System.Linq;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.Parameters;

namespace AspectCore.Extensions.DataValidation
{
    public sealed class DataValidationContext
    {
        public IEnumerable<DataValidationDescriptor> DataValidationDescriptors { get; }

        public AspectContext AspectContext { get; }

        public IDataState DataState { get; }

        public DataValidationContext(AspectContext aspectContext)
        {
            AspectContext = aspectContext;
            DataValidationDescriptors = aspectContext.GetParameters().Select(param => new DataValidationDescriptor(param)).ToArray();
        }
    }
}