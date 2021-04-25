using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.DataValidation
{
    /// <summary>
    /// 校验后的数据状态接口
    /// </summary>
    [NonAspect]
    public interface IDataState
    {
        /// <summary>
        /// 是否通过检验
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// 数据检验后的错误信息
        /// </summary>
        DataValidationErrorCollection Errors { get; }
    }
}