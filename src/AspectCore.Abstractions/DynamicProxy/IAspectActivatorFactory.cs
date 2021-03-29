namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 用以生成IAspectActivator对象的工厂接口
    /// </summary>
    [NonAspect]
    public interface IAspectActivatorFactory
    {
        /// <summary>
        /// 创建一个IAspectActivator对象,以执行拦截管道
        /// </summary>
        /// <returns>提供方法，用以触发执行拦截管道的对象</returns>
        IAspectActivator Create();
    }
}