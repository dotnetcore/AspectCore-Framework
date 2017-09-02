using System;
using System.Collections;
using System.Collections.Generic;

namespace AspectCore.Configuration
{
    public sealed class NonAspectPredicateCollection : IEnumerable<AspectPredicate>
    {
        private readonly ICollection<AspectPredicate> _collection = new List<AspectPredicate>();

        public NonAspectPredicateCollection Add(AspectPredicate interceptorFactory)
        {
            _collection.Add(interceptorFactory);
            return this;
        }

        public int Count => _collection.Count;

        public IEnumerator<AspectPredicate> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}