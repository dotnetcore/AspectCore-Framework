using System;
using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 用于提供一个作用域
    /// </summary>
    [NonAspect]
    internal class ScopeResolverFactory : IScopeResolverFactory
    {
        private readonly ServiceResolver _serviceResolver;

        /// <summary>
        /// 用于提供一个作用域
        /// </summary>
        /// <param name="serviceResolver">服务解析器</param>
        public ScopeResolverFactory(IServiceResolver serviceResolver)
        {
            _serviceResolver = serviceResolver as ServiceResolver;
        }

        /// <summary>
        ///  提供一个作用域
        /// </summary>
        /// <returns>作用域</returns>
        public IServiceResolver CreateScope()
        {
            if (_serviceResolver == null)
            {
                throw new ArgumentNullException("ServiceResolver");
            }
            return new ServiceResolver(_serviceResolver._root ?? _serviceResolver);
        }
    }
}