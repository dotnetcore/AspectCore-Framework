using System;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 提供属性注入功能
    /// </summary>
    internal sealed class PropertyInjector : IPropertyInjector
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly PropertyResolver[] _propertyResolvers;

        /// <summary>
        /// 提供属性注入功能
        /// </summary>
        /// <param name="serviceProvider">提供服务的IServiceProvider对象</param>
        /// <param name="propertyResolvers">解析属性的一组对象</param>
        public PropertyInjector(IServiceProvider serviceProvider, PropertyResolver[] propertyResolvers)
        {
            _serviceProvider = serviceProvider;
            _propertyResolvers = propertyResolvers;
        }

        /// <summary>
        /// 解析并注入[对象implementation中]需要进行属性注入的属性
        /// </summary>
        /// <param name="implementation">待属性注入的对象</param>
        public void Invoke(object implementation)
        {
            if (implementation == null)
            {
                return;
            }
            var resolverLength = _propertyResolvers.Length;
            if (resolverLength == 0)
            {
                return;
            }
            for (var i = 0; i < resolverLength; i++)
            {
                _propertyResolvers[i].Resolve(_serviceProvider, implementation);
            }
        }
    }
}