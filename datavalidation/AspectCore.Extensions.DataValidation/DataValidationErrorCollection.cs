using System;
using System.Collections;
using System.Collections.Generic;

namespace AspectCore.Extensions.DataValidation
{
    public sealed class DataValidationErrorCollection : IEnumerable<DataValidationError>, IEnumerable
    {
        private readonly HashSet<DataValidationError> collection = new HashSet<DataValidationError>();

        public bool Add(DataValidationError dataValidationError)
        {
            if (dataValidationError == null)
            {
                throw new ArgumentNullException(nameof(dataValidationError));
            }

            return collection.Add(dataValidationError);
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