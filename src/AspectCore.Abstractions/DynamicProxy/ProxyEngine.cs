namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// AOP 后端引擎选择。
    /// </summary>
    public enum ProxyEngine
    {
        /// <summary>
        /// 运行时 DynamicProxy Emit（默认）。
        /// </summary>
        DynamicProxy = 0,

        /// <summary>
        /// 编译期 Source Generator 生成的代理（需要生成物 registry）。
        /// </summary>
        SourceGenerator = 1,

        /// <summary>
        /// 优先 Source Generator，缺失时可按 AllowRuntimeFallback 策略回退 DynamicProxy。
        /// </summary>
        Auto = 2,
    }
}

