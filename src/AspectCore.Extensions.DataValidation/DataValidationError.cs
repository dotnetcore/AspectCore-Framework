using System;

namespace AspectCore.Extensions.DataValidation
{
    /// <summary>
    /// 数据校验错误
    /// </summary>
    public sealed class DataValidationError
    {
        /// <summary>
        /// 键
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// 数据校验错误信息
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// 数据校验错误异常
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// 数据校验错误
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="errorMessage">校验错误消息</param>
        public DataValidationError(string key, string errorMessage)
        {
            Key = key ?? string.Empty;
            ErrorMessage = errorMessage ?? string.Empty;
        }

        /// <summary>
        /// 数据校验错误
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="exception">校验错误异常</param>
        public DataValidationError(string key, Exception exception)
            : this(key, null, exception)
        {
        }

        /// <summary>
        /// 数据校验错误
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="errorMessage">校验错误消息</param>
        /// <param name="exception">校验错误异常</param>
        public DataValidationError(string key, string errorMessage, Exception exception)
            : this(key, errorMessage)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            Exception = exception;
        }
    }
}
