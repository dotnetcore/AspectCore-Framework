using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 提供瞬时服务
    /// </summary>
    [NonAspect]
    public interface ITransientServiceAccessor<T> where T : class
    {
        /// <summary>
        /// 瞬时服务对象
        /// </summary>
        T Value { get; }
    }
}