using System;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 包装IServiceProvider.GetService，获取服务
    /// </summary>
    /// <typeparam name="T">服务的类型</typeparam>
    public sealed class TransientServiceAccessor<T> : ITransientServiceAccessor<T> where T : class
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 从DI中获取的服务
        /// </summary>
        public T Value => (T)_serviceProvider.GetService(typeof(T));

        /// <summary>
        /// 包装IServiceProvider.GetService，获取服务
        /// </summary>
        /// <param name="serviceProvider">IServiceProvider</param>
        public TransientServiceAccessor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }
    }
}