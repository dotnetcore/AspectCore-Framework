using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.DataValidation
{
    public static class AspectContextExtensions
    {
        private const string DataValidationContextKey = "DataValidation-Context";

        /// <summary>
        /// 获取数据检验上下文
        /// </summary>
        /// <param name="aspectContext">拦截上下文</param>
        /// <returns>数据检验上下文</returns>
        public static DataValidationContext GetDataValidationContext(this AspectContext aspectContext)
        {
            if (aspectContext == null)
            {
                throw new ArgumentNullException(nameof(aspectContext));
            }
            return aspectContext.AdditionalData[DataValidationContextKey] as DataValidationContext;
        }

        /// <summary>
        /// 设置数据检验上下文
        /// </summary>
        /// <param name="aspectContext">拦截上下文</param>
        /// <param name="dataValidationContext">数据检验上下文</param>
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