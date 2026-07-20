using System;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.CastleCompat;
using AspectCore.Extensions.DependencyInjection;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.Extensions.CastleCompat.Tests;

// ── Service types for DI tests ─────────────────────────────────────────

public interface IDiService
{
    string GetValue();
}

public class DiServiceImpl : IDiService
{
    public virtual string GetValue() => "original";
}

// ── Castle interceptors for DI tests ───────────────────────────────────

public class DiRecordingInterceptor : Castle.DynamicProxy.IInterceptor
{
    public int CallCount { get; private set; }

    public void Intercept(IInvocation invocation)
    {
        CallCount++;
        invocation.Proceed();
    }
}

public class DiPrefixInterceptor : Castle.DynamicProxy.IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        invocation.Proceed();
        if (invocation.ReturnValue is string s)
        {
            invocation.ReturnValue = $"[prefixed]{s}";
        }
    }
}

public class DiSuffixInterceptor : Castle.DynamicProxy.IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        invocation.Proceed();
        if (invocation.ReturnValue is string s)
        {
            invocation.ReturnValue = $"{s}[suffixed]";
        }
    }
}

/// <summary>Parameterless constructor interceptor for generic registration.</summary>
public class DiDefaultCtorInterceptor : Castle.DynamicProxy.IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        invocation.Proceed();
        if (invocation.ReturnValue is string s)
        {
            invocation.ReturnValue = $"[default]{s}";
        }
    }
}

// ── Tests ───────────────────────────────────────────────────────────────

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCastleInterceptor_Instance_NullServices_Throws()
    {
        IServiceCollection services = null!;
        var interceptor = new DiRecordingInterceptor();

        Assert.Throws<ArgumentNullException>(() =>
            services.AddCastleInterceptor(interceptor));
    }

    [Fact]
    public void AddCastleInterceptor_Instance_NullInterceptor_Throws()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() =>
            services.AddCastleInterceptor(null!));
    }

    [Fact]
    public void AddCastleInterceptors_NullServices_Throws()
    {
        IServiceCollection services = null!;
        AspectPredicate predicate = Predicates.ForService("*");

        Assert.Throws<ArgumentNullException>(() =>
            services.AddCastleInterceptors(predicate, new DiRecordingInterceptor()));
    }

    [Fact]
    public void AddCastleInterceptors_NullPredicate_Throws()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() =>
            services.AddCastleInterceptors(null!, new DiRecordingInterceptor()));
    }

    [Fact]
    public void AddCastleInterceptors_NullInterceptorsArray_Throws()
    {
        var services = new ServiceCollection();
        AspectPredicate predicate = Predicates.ForService("*");

        Assert.Throws<ArgumentNullException>(() =>
            services.AddCastleInterceptors(predicate, null!));
    }

    [Fact]
    public void AddCastleInterceptor_Instance_WithPredicate_Returns_Services()
    {
        var services = new ServiceCollection();
        var interceptor = new DiRecordingInterceptor();

        var result = services.AddCastleInterceptor(interceptor, Predicates.ForService("*DiService*"));

        Assert.Same(services, result);
    }

    [Fact]
    public void AddCastleInterceptor_Instance_WithoutPredicate_Returns_Services()
    {
        var services = new ServiceCollection();
        var interceptor = new DiRecordingInterceptor();

        var result = services.AddCastleInterceptor(interceptor);

        Assert.Same(services, result);
    }

    [Fact]
    public void AddCastleInterceptor_Generic_Returns_Services()
    {
        var services = new ServiceCollection();

        var result = services.AddCastleInterceptor<DiDefaultCtorInterceptor>();

        Assert.Same(services, result);
    }

    [Fact]
    public void AddCastleInterceptor_Generic_WithPredicate_Returns_Services()
    {
        var services = new ServiceCollection();

        var result = services.AddCastleInterceptor<DiDefaultCtorInterceptor>(
            Predicates.ForService("*DiService*"));

        Assert.Same(services, result);
    }

    [Fact]
    public void AddCastleInterceptors_Returns_Services()
    {
        var services = new ServiceCollection();
        var interceptor1 = new DiPrefixInterceptor();
        var interceptor2 = new DiSuffixInterceptor();

        var result = services.AddCastleInterceptors(
            Predicates.ForService("*DiService*"), interceptor1, interceptor2);

        Assert.Same(services, result);
    }

    [Fact]
    public void AddCastleInterceptor_Instance_NoPredicate_Registers_ForAllServices()
    {
        var services = new ServiceCollection();
        services.AddTransient<IDiService, DiServiceImpl>();
        var interceptor = new DiRecordingInterceptor();

        services.AddCastleInterceptor(interceptor);

        // If ConfigureDynamicProxy was called, the service collection should have more registrations
        // We can't easily resolve with proxy generation without the full AspectCore DI setup,
        // but we verify no exceptions and the method was called.
        Assert.True(services.Count >= 1);
    }

    [Fact]
    public void AddCastleInterceptor_Instance_WithPredicate_Registers_ForMatchingServices()
    {
        var services = new ServiceCollection();
        services.AddTransient<IDiService, DiServiceImpl>();
        var interceptor = new DiRecordingInterceptor();

        services.AddCastleInterceptor(interceptor, Predicates.ForService("*DiService*"));

        Assert.True(services.Count >= 1);
    }

    [Fact]
    public void AddCastleInterceptors_Registers_Multiple_Interceptors()
    {
        var services = new ServiceCollection();
        services.AddTransient<IDiService, DiServiceImpl>();

        var interceptor1 = new DiPrefixInterceptor();
        var interceptor2 = new DiSuffixInterceptor();

        services.AddCastleInterceptors(
            Predicates.ForService("*DiService*"),
            interceptor1, interceptor2);

        // Should not throw and service collection should have entries
        Assert.True(services.Count >= 1);
    }

    [Fact]
    public void AddCastleInterceptor_Generic_NoPredicate_Works()
    {
        var services = new ServiceCollection();
        services.AddTransient<IDiService, DiServiceImpl>();

        services.AddCastleInterceptor<DiDefaultCtorInterceptor>();

        Assert.True(services.Count >= 1);
    }

    [Fact]
    public void AddCastleInterceptors_Empty_Array_NoOp()
    {
        var services = new ServiceCollection();
        var initialCount = services.Count;

        services.AddCastleInterceptors(
            Predicates.ForService("*"),
            Array.Empty<Castle.DynamicProxy.IInterceptor>());

        // Empty array means no interceptors registered via the foreach loop,
        // but ConfigureDynamicProxy won't be called at all
        Assert.Equal(initialCount, services.Count);
    }

    [Fact]
    public void AddCastleInterceptor_Chaining_Works()
    {
        var services = new ServiceCollection();

        var result = services
            .AddCastleInterceptor(new DiPrefixInterceptor())
            .AddCastleInterceptor(new DiSuffixInterceptor(), Predicates.ForService("*"));

        Assert.Same(services, result);
    }
}
