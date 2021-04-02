using System;
using System.Collections.Generic;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 代理生成器的构建者
    /// </summary>
    [NonAspect]
    public sealed class ProxyGeneratorBuilder
    {
        private readonly IAspectConfiguration _configuration;
        private readonly IServiceContext _serviceContext;

        /// <summary>
        /// 代理生成器的构建者
        /// </summary>
        public ProxyGeneratorBuilder()
        {
            _configuration = new AspectConfiguration();
            _serviceContext = new ServiceContext(_configuration);
        }

        /// <summary>
        /// 配置代理生成
        /// </summary>
        /// <param name="options">提供配置的委托</param>
        /// <returns>ProxyGeneratorBuilder</returns>
        public ProxyGeneratorBuilder Configure(Action<IAspectConfiguration> options = null)
        {
            options?.Invoke(_configuration);
            return this;
        }

        /// <summary>
        /// 配置服务，用于代理生成
        /// </summary>
        /// <param name="options">提供服务上下文的委托</param>
        /// <returns>ProxyGeneratorBuilder</returns>
        public ProxyGeneratorBuilder ConfigureService(Action<IServiceContext> options = null)
        {
            options?.Invoke(_serviceContext);
            return this;
        }

        /// <summary>
        /// 构建一个代理生成器
        /// </summary>
        /// <returns>代理生成器</returns>
        public IProxyGenerator Build()
        {
            var serviceResolver = _serviceContext.Build();
            return new DisposedProxyGenerator(serviceResolver);
        }
    }
}