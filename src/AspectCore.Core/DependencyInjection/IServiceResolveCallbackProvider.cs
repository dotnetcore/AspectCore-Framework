using System.Collections.Generic;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 服务解析回调功能的提供者接口
    /// </summary>
    internal interface IServiceResolveCallbackProvider
    {
        /// <summary>
        /// 提供一组服务回调功能
        /// </summary>
        IServiceResolveCallback[] ServiceResolveCallbacks { get; }
    }
}