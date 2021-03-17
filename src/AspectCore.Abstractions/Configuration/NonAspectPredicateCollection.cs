using System;
using System.Collections;
using System.Collections.Generic;

namespace AspectCore.Configuration
{
    /// <summary>
    /// 代表不拦截条件的集合，不拦截条件可添加到此对象中管理
    /// </summary>
    public sealed class NonAspectPredicateCollection : IEnumerable<AspectPredicate>
    {
        private readonly ICollection<AspectPredicate> _collection = new List<AspectPredicate>();

        /// <summary>
        /// 添加不拦截条件
        /// </summary>
        /// <param name="interceptorFactory">不拦截条件</param>
        /// <returns>不拦截条件集合对象</returns>
        public NonAspectPredicateCollection Add(AspectPredicate interceptorFactory)
        {
            _collection.Add(interceptorFactory);
            return this;
        }

        /// <summary>
        /// 此对象包含的不拦截条件的数量
        /// </summary>
        public int Count => _collection.Count;

        /// <summary>
        /// 获取此对象包含的不拦截条件的枚举器
        /// </summary>
        /// <returns></returns>
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