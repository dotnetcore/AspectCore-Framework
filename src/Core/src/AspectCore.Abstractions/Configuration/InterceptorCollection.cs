using System;
using System.Collections;
using System.Collections.Generic;

namespace AspectCore.Configuration
{
    public sealed class InterceptorCollection : IEnumerable<InterceptorFactory>
    {
        private readonly ICollection<InterceptorFactory> _collection = new List<InterceptorFactory>();

        public InterceptorCollection Add(InterceptorFactory interceptorFactory)
        {
            if (interceptorFactory == null)
            {
                throw new ArgumentNullException(nameof(interceptorFactory));
            }
            _collection.Add(interceptorFactory);
            return this;
        }

        public int Count => _collection.Count;

        public IEnumerator<InterceptorFactory> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}