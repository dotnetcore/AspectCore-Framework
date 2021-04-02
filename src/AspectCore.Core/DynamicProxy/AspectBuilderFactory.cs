using System;
using System.Reflection;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 拦截管道构建者的工厂
    /// </summary>
    [NonAspect]
    public sealed class AspectBuilderFactory : IAspectBuilderFactory
    {
        private readonly IInterceptorCollector _interceptorCollector;
        private readonly IAspectCaching _aspectCaching;

        /// <summary>
        /// 拦截管道构建者的工厂
        /// </summary>
        /// <param name="interceptorCollector">提供方法获取服务和实例上关联的拦截器</param>
        /// <param name="aspectCachingProvider">缓存提供器</param>
        public AspectBuilderFactory(IInterceptorCollector interceptorCollector,
            IAspectCachingProvider aspectCachingProvider)
        {
            if (aspectCachingProvider == null)
            {
                throw new ArgumentNullException(nameof(aspectCachingProvider));
            }
            _interceptorCollector =
                interceptorCollector ?? throw new ArgumentNullException(nameof(interceptorCollector));
            _aspectCaching = aspectCachingProvider.GetAspectCaching(nameof(AspectBuilderFactory));
        }

        /// <summary>
        /// 创建一个拦截管道构建者
        /// </summary>
        /// <param name="context">拦截上下文</param>
        /// <returns>拦截管道构建者</returns>
        public IAspectBuilder Create(AspectContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            return (IAspectBuilder)_aspectCaching.GetOrAdd(GetKey(context.ServiceMethod, context.ImplementationMethod), key => Create((Tuple<MethodInfo, MethodInfo>)key));
        }

        /// <summary>
        /// 创建一个拦截管道构建者
        /// </summary>
        /// <param name="tuple">暴露的服务方法和实现方法组合的Tuple对象</param>
        /// <returns>拦截管道构建者</returns>
        private IAspectBuilder Create(Tuple<MethodInfo, MethodInfo> tuple)
        {
            var aspectBuilder = new AspectBuilder(context => context.Complete(), null);

            foreach (var interceptor in _interceptorCollector.Collect(tuple.Item1, tuple.Item2))
                aspectBuilder.AddAspectDelegate(interceptor.Invoke);

            return aspectBuilder;
        }

        private object GetKey(MethodInfo serviceMethod, MethodInfo implementationMethod)
        {
            return Tuple.Create(serviceMethod, implementationMethod);
        }
    }
}