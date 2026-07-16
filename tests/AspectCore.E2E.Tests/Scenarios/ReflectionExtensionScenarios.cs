using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using AspectCore.Configuration;
using AspectCore.E2E.Tests.Fixtures;
using AspectCore.Extensions.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// E2E tests for reflection extension utilities on proxied types:
/// MethodReflector, PropertyReflector, ConstructorReflector, and
/// CustomAttributeReflector. Real proxy types, real reflection — no mocks.
/// </summary>
[Collection("InterceptorLog")]
public class ReflectionExtensionScenarios
{
    [Fact]
    public void MethodReflector_Invoke_OnProxyMethod_Works()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        // Get a MethodReflector for the Add method on the proxy type.
        var method = service.GetType().GetMethod(nameof(ICalculatorService.Add));
        Assert.NotNull(method);

        var reflector = method!.GetReflector();
        var result = reflector.Invoke(service, 3, 4);

        Assert.Equal(7, result);
    }

    [Fact]
    public void MethodReflector_Invoke_GenericMethod_OnProxy_Works()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        // Generic methods must be closed (type arguments specified) before
        // they can be invoked via MethodReflector.
        var method = service.GetType().GetMethods()
            .FirstOrDefault(m => m.Name == nameof(ICalculatorService.Echo) && m.IsGenericMethod);
        Assert.NotNull(method);

        var closedMethod = method!.MakeGenericMethod(typeof(int));
        var reflector = closedMethod.GetReflector();
        var result = reflector.Invoke(service, 42);

        Assert.Equal(42, result);
    }

    [Fact]
    public void PropertyReflector_GetValue_OnProxyProperty_Works()
    {
        using var host = new TestHost();
        host.Add<IPropertyService, PropertyService>();

        var service = host.Resolve<IPropertyService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IPropertyService)));
        });

        // Set a value via the proxy, then read it back via PropertyReflector.
        service.Name = "test-name";

        var property = service.GetType().GetProperty(nameof(IPropertyService.Name));
        Assert.NotNull(property);

        var reflector = property!.GetReflector();
        var value = reflector.GetValue(service);

        Assert.Equal("test-name", value);
    }

    [Fact]
    public void PropertyReflector_SetValue_OnProxyProperty_Works()
    {
        using var host = new TestHost();
        host.Add<IPropertyService, PropertyService>();

        var service = host.Resolve<IPropertyService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IPropertyService)));
        });

        var property = service.GetType().GetProperty(nameof(IPropertyService.Name));
        Assert.NotNull(property);

        var reflector = property!.GetReflector();
        reflector.SetValue(service, "set-via-reflector");

        Assert.Equal("set-via-reflector", service.Name);
    }

    [Fact]
    public void ConstructorReflector_Invoke_CreatesInstance()
    {
        var constructor = typeof(CalculatorService).GetConstructor(Type.EmptyTypes);
        Assert.NotNull(constructor);

        var reflector = constructor!.GetReflector();
        var instance = reflector.Invoke();

        Assert.NotNull(instance);
        Assert.IsType<CalculatorService>(instance);
    }

    [Fact]
    public void CustomAttributeReflector_OnProxyType_ReadsAttributes()
    {
        using var host = new TestHost();
        host.Add<IAttributeService, AttributeService>();

        var service = host.Resolve<IAttributeService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IAttributeService)));
        });

        // Read custom attributes from the proxy method via reflection extensions.
        var method = service.GetType().GetMethod(nameof(IAttributeService.DoWork));
        Assert.NotNull(method);

        var attributes = method!.GetReflector().GetCustomAttributes();
        var descriptionAttr = attributes.OfType<DescriptionAttribute>().FirstOrDefault();

        Assert.NotNull(descriptionAttr);
        Assert.Equal("method-desc", descriptionAttr!.Description);
    }

    [Fact]
    public void CustomAttributeReflector_OnProxyType_TypeLevelAttributes()
    {
        using var host = new TestHost();
        host.Add<AttributeClassService>();

        var service = host.Resolve<AttributeClassService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.ForService(nameof(AttributeClassService)));
        });

        // Read custom attributes from the proxy type.
        var attributes = service.GetType().GetReflector().GetCustomAttributes();
        var descriptionAttr = attributes.OfType<DescriptionAttribute>().FirstOrDefault();

        // The type-level Description attribute from the service class is forwarded.
        Assert.NotNull(descriptionAttr);
        Assert.Equal("class-type-desc", descriptionAttr!.Description);
    }

    /// <summary>
    /// Service with a settable property for PropertyReflector tests.
    /// </summary>
    public interface IPropertyService
    {
        string? Name { get; set; }
    }

    /// <summary>
    /// Real implementation with a virtual property.
    /// </summary>
    public class PropertyService : IPropertyService
    {
        public virtual string? Name { get; set; }
    }

    /// <summary>
    /// Service with description attributes for CustomAttributeReflector tests.
    /// </summary>
    [Description("type-desc")]
    public interface IAttributeService
    {
        [Description("method-desc")]
        void DoWork();
    }

    /// <summary>
    /// Real implementation of IAttributeService.
    /// </summary>
    public class AttributeService : IAttributeService
    {
        public virtual void DoWork() { }
    }

    /// <summary>
    /// Class service with a type-level Description attribute for class proxy tests.
    /// </summary>
    [Description("class-type-desc")]
    public class AttributeClassService
    {
        [Description("class-method-desc")]
        public virtual void DoWork() { }
    }
}
