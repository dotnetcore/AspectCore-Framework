using System;
using System.Collections;
using System.Collections.Generic;

namespace AspectCore.Extensions.DataValidation
{
    public sealed class DataValidationErrorCollection : IEnumerable<DataValidationError>, IEnumerable
    {
        private readonly ICollection<DataValidationError> collection = new List<DataValidationError>();

        public int Count
        {
            get
            {
                return collection.Count;
            }
        }

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