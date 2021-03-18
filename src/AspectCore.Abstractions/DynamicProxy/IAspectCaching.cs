using System;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 在AspectCore中提供缓存的一个接口定义
    /// </summary>
    [NonAspect]
    public interface IAspectCaching : IDisposable
    {
        /// <summary>
        /// 名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 获取缓存值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>值</returns>
        object Get(object key);

        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        void Set(object key, object value);

        /// <summary>
        /// 获取缓存值，如果没有就添加并返回
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="factory">如果缓存中没有,则使用此委托创建并返回</param>
        /// <returns>值</returns>
        object GetOrAdd(object key, Func<object, object> factory);
    }
}
