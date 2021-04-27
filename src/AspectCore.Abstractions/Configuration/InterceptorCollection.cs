using System;
using System.Collections;
using System.Collections.Generic;

namespace AspectCore.Configuration
{
    /// <summary>
    /// 代表拦截器工厂集合的对象
    /// </summary>
    public sealed class InterceptorCollection : IEnumerable<InterceptorFactory>
    {
        private readonly ICollection<InterceptorFactory> _collection = new List<InterceptorFactory>();

        /// <summary>
        /// 添加拦截器工厂
        /// </summary>
        /// <param name="interceptorFactory">拦截器工厂</param>
        /// <returns>InterceptorCollection</returns>
        public InterceptorCollection Add(InterceptorFactory interceptorFactory)
        {
            if (interceptorFactory == null)
            {
                throw new ArgumentNullException(nameof(interceptorFactory));
            }
            _collection.Add(interceptorFactory);
            return this;
        }

        /// <summary>
        /// 拦截器工厂的数量
        /// </summary>
        public int Count => _collection.Count;

        /// <summary>
        /// 迭代InterceptorCollection包含的拦截工厂
        /// </summary>
        /// <returns>迭代器,用以迭代此对象包含的拦截工厂</returns>
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