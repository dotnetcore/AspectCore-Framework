using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 继承此接口用以提供属性注入服务
    /// </summary>
    [NonAspect]
    public interface IPropertyInjector
    {
        /// <summary>
        /// implementation对象的属性如何被解析注入
        /// </summary>
        /// <param name="implementation">要被解析注入属性的对象</param>
        void Invoke(object implementation);
    }
}
