using System;

namespace AspectCore.Extensions.DataValidation
{
    public sealed class DataValidationError
    {
        public string Key { get; }

        public string ErrorMessage { get; }

        public Exception Exception { get; }

        public DataValidationError(string key, string errorMessage)
        {
            Key = key ?? string.Empty;
            ErrorMessage = errorMessage ?? string.Empty;
        }

        public DataValidationError(string key, Exception exception)
            : this(key, null, exception)
        {
        }

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
