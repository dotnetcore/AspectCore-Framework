using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 在AspectCore中提供缓存
    /// </summary>
    [NonAspect]
    internal class AspectCaching : IAspectCaching
    {
        private readonly ConcurrentDictionary<object, object> _dictionary;

        /// <summary>
        /// 在AspectCore中提供缓存
        /// </summary>
        /// <param name="name">名称</param>
        public AspectCaching(string name)
        {
            Name = name;
            _dictionary = new ConcurrentDictionary<object, object>();
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            foreach (var key in _dictionary.Keys.ToArray())
            {
                if (_dictionary.TryRemove(key, out var value))
                {
                    var enumerbale = value as IEnumerable;
                    if (enumerbale != null)
                    {
                        foreach (var item in enumerbale)
                        {
                            var d = item as IDisposable;
                            d?.Dispose();
                        }
                    }
                    var disposable = value as IDisposable;
                    disposable?.Dispose();
                }
            }
        }

        /// <summary>
        /// 获取缓存值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>值</returns>
        public object Get(object key)
        {
            return _dictionary[key];
        }

        /// <summary>
        /// 获取缓存值，如果没有就添加并返回
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="factory">如果缓存中没有,则使用此委托创建并返回</param>
        /// <returns>值</returns>
        public object GetOrAdd(object key, Func<object, object> factory)
        {
            return _dictionary.GetOrAdd(key, factory);
        }

        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void Set(object key, object value)
        {
            _dictionary[key] = value;
        }
    }
}