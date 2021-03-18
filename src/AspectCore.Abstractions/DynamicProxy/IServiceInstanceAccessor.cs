namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 服务实例访问
    /// </summary>
    [NonAspect]
    public interface IServiceInstanceAccessor
    {
        /// <summary>
        /// 服务实例
        /// </summary>
        object ServiceInstance { get; }
    }

    /// <summary>
    /// 服务实例访问
    /// </summary>
    [NonAspect]
    public interface IServiceInstanceAccessor<TService>
    {
        /// <summary>
        /// 服务实例
        /// </summary>
        TService ServiceInstance { get; }
    }
}
