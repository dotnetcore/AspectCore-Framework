using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.DataValidation
{
    public static class AspectContextExtensions
    {
        private const string DataValidationContextKey = "DataValidation-Context";

        public static DataValidationContext GetDataValidationContext(this AspectContext aspectContext)
        {
            if (aspectContext == null)
            {
                throw new ArgumentNullException(nameof(aspectContext));
            }
            return aspectContext.AdditionalData[DataValidationContextKey] as DataValidationContext;
        }

        public static void SetDataValidationContext(this AspectContext aspectContext, DataValidationContext dataValidationContext)
        {
            if (aspectContext == null)
            {
                throw new ArgumentNullException(nameof(aspectContext));
            }
            if (dataValidationContext == null)
            {
                throw new ArgumentNullException(nameof(dataValidationContext));
            }
            aspectContext.AdditionalData[DataValidationContextKey] = dataValidationContext;
        }
    }
}