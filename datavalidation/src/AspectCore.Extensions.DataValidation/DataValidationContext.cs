using System.Collections.Generic;
using System.Linq;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.Parameters;

namespace AspectCore.Extensions.DataValidation
{
    public sealed class DataValidationContext
    {
        public IEnumerable<DataMetaData> DataMetaDatas { get; }

        public AspectContext AspectContext { get; }

        public DataValidationContext(AspectContext aspectContext)
        {
            AspectContext = aspectContext;
            DataMetaDatas = aspectContext.GetParameters().Select(param => new DataMetaData(param)).ToArray();
        }
    }
}