using System;
using AspectCore.Configuration;
using AspectCore.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.E2E.Tests.Fixtures;

/// <summary>
/// Simulates real application startup for E2E tests.
/// Builds a real DI container, applies AOP configuration, and produces a
/// dynamic-proxy-enabled service provider — no mocks, no fakes.
/// </summary>
public sealed class TestHost : IDisposable
{
    private readonly ServiceCollection _services;
    private IServiceProvider? _provider;
    private bool _disposed;

    public TestHost()
    {
        _services = new ServiceCollection();
    }

    /// <summary>
    /// Direct access to the underlying service collection for advanced registration.
    /// </summary>
    public IServiceCollection Services => _services;

    /// <summary>
    /// Registers a service implementation against an interface and returns the host
    /// for fluent chaining.
    /// </summary>
    public TestHost Add<TService, TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TService : class
        where TImplementation : class, TService
    {
        EnsureNotBuilt();
        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                _services.AddSingleton<TService, TImplementation>();
                break;
            case ServiceLifetime.Transient:
                _services.AddTransient<TService, TImplementation>();
                break;
            default:
                _services.AddScoped<TService, TImplementation>();
                break;
        }
        return this;
    }

    /// <summary>
    /// Registers a service using a factory delegate and returns the host for fluent chaining.
    /// </summary>
    public TestHost Add<TService>(Func<IServiceProvider, TService> factory, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TService : class
    {
        EnsureNotBuilt();
        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                _services.AddSingleton(factory);
                break;
            case ServiceLifetime.Transient:
                _services.AddTransient(factory);
                break;
            default:
                _services.AddScoped(factory);
                break;
        }
        return this;
    }

    /// <summary>
    /// Registers a concrete class service (for class-proxy scenarios) and returns the host
    /// for fluent chaining.
    /// </summary>
    public TestHost Add<TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TImplementation : class
    {
        EnsureNotBuilt();
        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                _services.AddSingleton<TImplementation>();
                break;
            case ServiceLifetime.Transient:
                _services.AddTransient<TImplementation>();
                break;
            default:
                _services.AddScoped<TImplementation>();
                break;
        }
        return this;
    }

    /// <summary>
    /// Builds the service provider, applying the optional AOP configuration.
    /// This simulates the real application-startup pipeline:
    /// ServiceCollection → ConfigureDynamicProxy → BuildDynamicProxyProvider.
    /// </summary>
    public IServiceProvider CreateServiceProvider(Action<IAspectConfiguration>? configure = null)
    {
        if (_provider != null)
        {
            return _provider;
        }

        _services.ConfigureDynamicProxy(configure ?? (_ => { }));
        _provider = _services.BuildDynamicProxyProvider();
        return _provider;
    }

    /// <summary>
    /// Builds the service provider with explicit validation options.
    /// </summary>
    public IServiceProvider CreateServiceProvider(ServiceProviderOptions options, Action<IAspectConfiguration>? configure = null)
    {
        if (_provider != null)
        {
            return _provider;
        }

        _services.ConfigureDynamicProxy(configure ?? (_ => { }));
        _provider = _services.BuildDynamicProxyProvider(options);
        return _provider;
    }

    /// <summary>
    /// Resolves a service from the built provider, building it with default configuration
    /// if necessary.
    /// </summary>
    public TService Resolve<TService>(Action<IAspectConfiguration>? configure = null)
        where TService : notnull
    {
        var provider = CreateServiceProvider(configure);
        return provider.GetRequiredService<TService>();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        (_provider as IDisposable)?.Dispose();
    }

    private void EnsureNotBuilt()
    {
        if (_provider != null)
        {
            throw new InvalidOperationException("The service provider has already been built.");
        }
    }
}
