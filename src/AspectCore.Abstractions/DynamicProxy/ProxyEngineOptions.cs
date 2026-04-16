namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 控制 AOP 后端引擎（DynamicProxy / Source Generator）选择与回退策略。
    /// </summary>
    public sealed class ProxyEngineOptions
    {
        /// <summary>
        /// 默认 DynamicProxy，保持既有行为。
        /// </summary>
        public ProxyEngine Engine { get; set; } = ProxyEngine.DynamicProxy;

        /// <summary>
        /// 是否允许在缺失生成物时回退到运行时 DynamicProxy。
        /// 
        /// - Engine=Auto：默认 true
        /// - Engine=SourceGenerator：默认 false
        /// </summary>
        public bool? AllowRuntimeFallback { get; set; }

        /// <summary>
        /// 为 true 时，缺失生成物将抛出异常（用于 CI 强约束覆盖率）。
        /// </summary>
        public bool Strict { get; set; }
    }
}

