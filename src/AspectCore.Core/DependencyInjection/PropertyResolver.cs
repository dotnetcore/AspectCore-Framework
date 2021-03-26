using System;
using AspectCore.Extensions.Reflection;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 属性解析
    /// </summary>
    public sealed class PropertyResolver
    {
        private readonly Func<IServiceProvider, object> _propertyFactory;
        private readonly PropertyReflector _reflector;

        /// <summary>
        /// 构造属性解析对象
        /// </summary>
        /// <param name="propertyFactory">通过此委托创建属性对象</param>
        /// <param name="reflector">属性反射支持对象</param>
        internal PropertyResolver(Func<IServiceProvider, object> propertyFactory, PropertyReflector reflector)
        {  
            _propertyFactory = propertyFactory;
            _reflector = reflector;
        }

        /// <summary>
        /// 解析属性
        /// </summary>
        /// <param name="provider">服务提供者</param>
        /// <param name="implementation">实例对象</param>
        public void Resolve(IServiceProvider provider, object implementation)
        {
            _reflector.SetValue(implementation, _propertyFactory(provider));
        }
    }
}