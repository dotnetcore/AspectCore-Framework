using System.Collections.Generic;
using System.Linq;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.Parameters;

namespace AspectCore.Extensions.DataValidation
{
    /// <summary>
    /// 数据校验上下文
    /// </summary>
    public sealed class DataValidationContext
    {
        /// <summary>
        /// 用于校验的数据元数据信息
        /// </summary>
        public IEnumerable<DataMetaData> DataMetaDatas { get; }

        /// <summary>
        /// 拦截上下文
        /// </summary>
        public AspectContext AspectContext { get; }

        /// <summary>
        /// 数据校验上下文
        /// </summary>
        /// <param name="aspectContext">拦截上下文</param>
        public DataValidationContext(AspectContext aspectContext)
        {
            AspectContext = aspectContext;
            DataMetaDatas = aspectContext.GetParameters().Select(param => new DataMetaData(param)).ToArray();
        }
    }
}