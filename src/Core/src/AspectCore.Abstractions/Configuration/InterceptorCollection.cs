using System;
using System.Collections;
using System.Collections.Generic;

namespace AspectCore.Abstractions
{
    public sealed class InterceptorCollection : IEnumerable<IInterceptorFactory>
    {
        private readonly ICollection<IInterceptorFactory> _collection = new List<IInterceptorFactory>();

        public InterceptorCollection Add(IInterceptorFactory interceptorFactory)
        {
            if (interceptorFactory == null)
            {
                throw new ArgumentNullException(nameof(interceptorFactory));
            }
            _collection.Add(interceptorFactory);
            return this;
        }

        public int Count => _collection.Count;

        public IEnumerator<IInterceptorFactory> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}