using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 提供一个作用域
    /// </summary>
    [NonAspect]
    public interface IScopeResolverFactory
    {
        /// <summary>
        /// 提供一个作用域
        /// </summary>
        /// <returns>作用域</returns>
        IServiceResolver CreateScope();
    }
}