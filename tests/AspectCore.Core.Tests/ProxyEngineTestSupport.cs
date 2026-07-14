using System;
using System.Collections.Generic;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;

namespace AspectCore.Core.Tests;

internal static class ProxyEngineTestSupport
{
    public static IEnumerable<object[]> Engines()
    {
        yield return new object[] { ProxyEngine.DynamicProxy };
        yield return new object[] { ProxyEngine.SourceGenerator };
    }

    public static IProxyGenerator CreateProxyGenerator(
        ProxyEngine engine,
        Action<IAspectConfiguration> configureAspect,
        bool strict = false,
        bool? allowRuntimeFallback = null,
        Action<IServiceContext> configureService = null)
    {
        if (configureAspect is null) throw new ArgumentNullException(nameof(configureAspect));

        var engineOptions = new ProxyEngineOptions
        {
            Engine = engine,
            Strict = strict,
            AllowRuntimeFallback = allowRuntimeFallback,
        };

        var builder = new ProxyGeneratorBuilder();
        builder.Configure(configureAspect);
        builder.ConfigureService(serviceContext =>
        {
            if (engine != ProxyEngine.DynamicProxy || strict || allowRuntimeFallback.HasValue)
            {
                // Ensure ProxyEngineOptions is available for DI paths and diagnostics.
                serviceContext.AddInstance(engineOptions);
            }

            // For direct IProxyGenerator usage, override the default IProxyTypeGenerator registration.
            // (ServiceTable also has its own selection logic, but ProxyGenerator resolves IProxyTypeGenerator from DI.)
            if (engine != ProxyEngine.DynamicProxy)
            {
                serviceContext.RemoveAll(typeof(IProxyTypeGenerator));
                serviceContext.AddInstance<IProxyTypeGenerator>(
                    new SourceGeneratedProxyTypeGenerator(
                        new AspectValidatorBuilder(serviceContext.Configuration),
                        engineOptions,
                        Array.Empty<ISourceGeneratedProxyRegistry>()));
            }

            configureService?.Invoke(serviceContext);
        });

        return builder.Build();
    }
}
