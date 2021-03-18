using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 实现属性注入功能的接口
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
