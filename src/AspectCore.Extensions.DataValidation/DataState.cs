using System;
using System.Collections.Generic;
using System.Text;

namespace AspectCore.Extensions.DataValidation
{
    internal class DataState : IDataState
    {
        private readonly bool _isDataValid;

        public bool IsValid
        {
            get
            {
                return _isDataValid && Errors.Count == 0;
            }
        }

        public DataValidationErrorCollection Errors { get; }

        public DataState(bool isValid, DataValidationErrorCollection dataValidationErrors)
        {
            _isDataValid = isValid;
            Errors = dataValidationErrors ?? throw new ArgumentNullException(nameof(dataValidationErrors));
        }
    }
}