using System.Collections.Generic;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 用于提供一组服务回调功能
    /// </summary>
    internal interface IServiceResolveCallbackProvider
    {
        /// <summary>
        /// 一组服务回调功能
        /// </summary>
        IServiceResolveCallback[] ServiceResolveCallbacks { get; }
    }
}