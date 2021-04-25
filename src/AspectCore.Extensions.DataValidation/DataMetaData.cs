using System;
using System.Linq;
using AspectCore.DynamicProxy.Parameters;
using AspectCore.Extensions.Reflection;
using System.ComponentModel.DataAnnotations;

namespace AspectCore.Extensions.DataValidation
{
    /// <summary>
    /// 用于数据校验的元数据信息
    /// </summary>
    public sealed class DataMetaData
    {
        /// <summary>
        /// 参数数据类型
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// 标注上参数上的特性
        /// </summary>
        public Attribute[] Attributes { get; }

        /// <summary>
        /// 参数值
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// 校验的错误信息
        /// </summary>
        public DataValidationErrorCollection Errors { get; }

        /// <summary>
        /// 校验状态
        /// </summary>
        public DataValidationState State { get; set; }

        /// <summary>
        /// 用于数据校验的元数据信息
        /// </summary>
        /// <param name="paramter">参数对象</param>
        public DataMetaData(Parameter paramter)
        {
            Type = paramter.Type;
            Value = paramter.Value;
            Attributes = paramter.ParameterInfo.GetReflector().GetCustomAttributes();
            Errors = new DataValidationErrorCollection();
        }
    }
}