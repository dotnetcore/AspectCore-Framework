using System;
using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 创建一个对象以提供属性注入功能
    /// </summary>
    [NonAspect]
    public class PropertyInjectorFactory : IPropertyInjectorFactory
    {
        private readonly IServiceProvider _servicePorvider;
        private readonly PropertyResolverSelector _propertyResolverSelector;

        /// <summary>
        /// 创建一个对象以提供属性注入功能
        /// </summary>
        /// <param name="servicePorvider">服务提供者</param>
        public PropertyInjectorFactory(IServiceProvider servicePorvider)
        {
            _servicePorvider = servicePorvider;
            _propertyResolverSelector = PropertyResolverSelector.Default;
        }

        /// <summary>
        /// 创建一个对象以提供属性注入功能
        /// </summary>
        /// <param name="implementationType">待操作对象</param>
        /// <returns>提供属性注入功能的对象</returns>
        public IPropertyInjector Create(Type implementationType)
        {
            return new PropertyInjector(_servicePorvider, _propertyResolverSelector.SelectPropertyResolver(implementationType));
        }
    }
}