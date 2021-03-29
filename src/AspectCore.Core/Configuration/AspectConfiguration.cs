using AspectCore.DependencyInjection;

namespace AspectCore.Configuration
{
    /// <summary>
    /// AspectCore的配置对象
    /// </summary>
    public sealed class AspectConfiguration : IAspectConfiguration
    {
        /// <summary>
        /// 验证检查处理器集合
        /// </summary>
        public AspectValidationHandlerCollection ValidationHandlers { get; }

        /// <summary>
        /// 拦截器工厂集合
        /// </summary>
        public InterceptorCollection Interceptors { get; }

        /// <summary>
        /// 不拦截条件集合
        /// </summary>
        public NonAspectPredicateCollection NonAspectPredicates { get; }

        /// <summary>
        /// 是否抛出拦截异常
        /// </summary>
        public bool ThrowAspectException { get; set; }

        /// <summary>
        /// AspectCore的配置对象
        /// </summary>
        public AspectConfiguration()
        {
            ThrowAspectException = false;
            ValidationHandlers = new AspectValidationHandlerCollection().AddDefault(this);
            Interceptors = new InterceptorCollection();
            NonAspectPredicates = new NonAspectPredicateCollection().AddDefault();
        }
    }
}