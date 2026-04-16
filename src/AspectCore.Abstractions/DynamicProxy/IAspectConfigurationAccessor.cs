using AspectCore.Configuration;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 为 Source Generator 代理提供访问 IAspectConfiguration 的能力，用于运行时构建 IAspectValidator，
    /// 从而对齐 DynamicProxy "是否需要拦截" 的决策（避免无拦截时仍走 AspectActivator 导致异常包装等语义变化）。
    /// </summary>
    public interface IAspectConfigurationAccessor
    {
        IAspectConfiguration AspectConfiguration { get; }
    }
}

