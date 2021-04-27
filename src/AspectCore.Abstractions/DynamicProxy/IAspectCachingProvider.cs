using System;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 缓存提供器接口
    /// </summary>
    [NonAspect]
    public interface IAspectCachingProvider : IDisposable
    {
        /// <summary>
        /// 通过name获取缓存
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>获取到的缓存</returns>
        IAspectCaching GetAspectCaching(string name);
    }
}