using System;
using System.Collections.Concurrent;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 缓存提供器
    /// </summary>
    [NonAspect]
    public sealed class AspectCachingProvider : IAspectCachingProvider
    {
        private readonly ConcurrentDictionary<string, IAspectCaching> _cachings;

        /// <summary>
        /// 缓存提供器
        /// </summary>
        public AspectCachingProvider()
        {
            _cachings = new ConcurrentDictionary<string, IAspectCaching>();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            foreach(var caching in _cachings)
            {
                caching.Value.Dispose();
            }
            _cachings.Clear();
        }

        /// <summary>
        /// 通过name获取缓存
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>获取到的缓存</returns>
        public IAspectCaching GetAspectCaching(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            return _cachings.GetOrAdd(name, key => new AspectCaching(key));
        }
    }
}