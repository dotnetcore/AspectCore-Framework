using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.DataValidation
{
    /// <summary>
    /// 提供数据校验状态IDataState
    /// </summary>
    [NonAspect]
    public interface IDataStateProvider
    {
        /// <summary>
        /// 校验后的数据状态接口
        /// </summary>
        IDataState DataState { get; set; }
    }
}