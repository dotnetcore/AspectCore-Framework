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
        /// <param name="serviceProvider">IServiceProvider</param>
        /// <param name="propertyResolvers">一组解析属性的对象</param>
        public PropertyInjector(IServiceProvider serviceProvider, PropertyResolver[] propertyResolvers)
        {
            _serviceProvider = serviceProvider;
            _propertyResolvers = propertyResolvers;
        }

        /// <summary>
        /// 依赖注入对象的属性
        /// </summary>
        /// <param name="implementation">操作的对象</param>
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