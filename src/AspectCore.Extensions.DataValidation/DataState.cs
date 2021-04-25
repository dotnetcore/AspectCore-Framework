using System;
using System.Collections.Generic;
using System.Text;

namespace AspectCore.Extensions.DataValidation
{
    /// <summary>
    /// 校验后的数据状态
    /// </summary>
    internal class DataState : IDataState
    {
        private readonly bool _isDataValid;

        /// <summary>
        /// 是否通过检验
        /// </summary>
        public bool IsValid
        {
            get
            {
                return _isDataValid && Errors.Count == 0;
            }
        }

        /// <summary>
        /// 数据检验后的错误信息
        /// </summary>
        public DataValidationErrorCollection Errors { get; }

        /// <summary>
        /// 校验后的数据状态
        /// </summary>
        /// <param name="isValid">是否通过检验</param>
        /// <param name="dataValidationErrors">数据检验后的错误信息</param>
        public DataState(bool isValid, DataValidationErrorCollection dataValidationErrors)
        {
            _isDataValid = isValid;
            Errors = dataValidationErrors ?? throw new ArgumentNullException(nameof(dataValidationErrors));
        }
    }
}