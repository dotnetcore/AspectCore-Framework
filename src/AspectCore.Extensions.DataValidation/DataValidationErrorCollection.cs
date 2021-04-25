using System;
using System.Collections;
using System.Collections.Generic;

namespace AspectCore.Extensions.DataValidation
{
    /// <summary>
    /// 数据校验错误集合
    /// </summary>
    public sealed class DataValidationErrorCollection : IEnumerable<DataValidationError>, IEnumerable
    {
        private readonly ICollection<DataValidationError> collection = new List<DataValidationError>();

        /// <summary>
        /// 数据校验错误数量
        /// </summary>
        public int Count
        {
            get
            {
                return collection.Count;
            }
        }

        /// <summary>
        /// 添加校验错误
        /// </summary>
        /// <param name="dataValidationError">数据校验错误</param>
        public void Add(DataValidationError dataValidationError)
        {
            if (dataValidationError == null)
            {
                throw new ArgumentNullException(nameof(dataValidationError));
            }
            collection.Add(dataValidationError);
        }

        public IEnumerator<DataValidationError> GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}