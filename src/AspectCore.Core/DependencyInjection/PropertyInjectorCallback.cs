namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 获取服务后对对象进行属性注入
    /// </summary>
    public class PropertyInjectorCallback : IServiceResolveCallback
    {
        /// <summary>
        /// 获取服务后对对象进行属性注入
        /// </summary>
        /// <param name="resolver">服务解析</param>
        /// <param name="instance">待处理的对象</param>
        /// <param name="service">服务描述</param>
        /// <returns></returns>
        public object Invoke(IServiceResolver resolver, object instance, ServiceDefinition service)
        {
            if (instance == null || !service.RequiredPropertyInjection())
            {
                return instance;
            }
            var injectorFactory = resolver.Resolve<IPropertyInjectorFactory>();
            var injector = injectorFactory.Create(instance.GetType());
            injector.Invoke(instance);
            return instance;
        }
    }
}