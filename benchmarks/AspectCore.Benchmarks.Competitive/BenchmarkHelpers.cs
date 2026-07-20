using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;

namespace AspectCore.Benchmarks.Competitive;

/// <summary>
/// Shared helper methods for benchmark setup.
/// </summary>
internal static class BenchmarkHelpers
{
    public static IProxyGenerator CreateAspectCoreGenerator(ProxyEngine engine)
    {
        var options = new ProxyEngineOptions
        {
            Engine = engine,
            Strict = engine == ProxyEngine.SourceGenerator,
            AllowRuntimeFallback = engine == ProxyEngine.SourceGenerator ? false : (bool?)null,
        };

        var builder = new ProxyGeneratorBuilder();
        builder.Configure(cfg =>
        {
            cfg.Interceptors.AddTyped<AspectCorePassthroughInterceptor>(
                Predicates.ForService("*Calculator*"));
        });
        builder.ConfigureService(sc =>
        {
            sc.AddInstance(options);
            if (engine != ProxyEngine.DynamicProxy)
            {
                sc.RemoveAll(typeof(IProxyTypeGenerator));
                sc.AddInstance<IProxyTypeGenerator>(
                    new SourceGeneratedProxyTypeGenerator(
                        new AspectValidatorBuilder(sc.Configuration),
                        options,
                        Array.Empty<ISourceGeneratedProxyRegistry>()));
            }
        });
        return builder.Build();
    }
}
