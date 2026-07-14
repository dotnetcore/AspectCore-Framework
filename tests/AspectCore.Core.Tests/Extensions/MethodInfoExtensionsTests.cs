#nullable enable
#pragma warning disable IDE0060 // Remove unused parameter
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Extensions;
using Xunit;
using static AspectCore.Core.Tests.Extensions.TypeExtensionsTests.TestTypes;

namespace AspectCore.Core.Tests.Extensions;

public class MethodInfoExtensionsTests
{
    // -------------------------------------------------------------------
    //  GetInterfaceDeclarations
    // -------------------------------------------------------------------

    [Fact]
    public void GetInterfaceDeclarations_ShouldReturnInterfaceMethod_WhenClassImplementsInterface()
    {
        var method = typeof(CommonService).GetMethod(nameof(CommonService.Method))!;

        var declarations = method.GetInterfaceDeclarations().ToList();

        Assert.Single(declarations);
        Assert.Equal(typeof(ICommonService).GetMethod(nameof(ICommonService.Method)), declarations[0]);
    }

    [Fact]
    public void GetInterfaceDeclarations_ShouldReturnInterfaceGetter_WhenPropertyImplementsInterface()
    {
        var getter = typeof(CommonService).GetProperty(nameof(CommonService.Property))!.GetMethod!;

        var declarations = getter.GetInterfaceDeclarations().ToList();

        Assert.Single(declarations);
        Assert.Equal(typeof(ICommonService).GetProperty(nameof(ICommonService.Property))!.GetMethod, declarations[0]);
    }

    [Fact]
    public void GetInterfaceDeclarations_ShouldReturnMultipleDeclarations_WhenMethodImplementsMultipleInterfaces()
    {
        var method = typeof(MultiInterfaceImpl).GetMethod(nameof(MultiInterfaceImpl.Shared))!;

        var declarations = method.GetInterfaceDeclarations().ToList();

        Assert.Equal(2, declarations.Count);
        Assert.Contains(typeof(ISharedOne).GetMethod(nameof(ISharedOne.Shared)), declarations);
        Assert.Contains(typeof(ISharedTwo).GetMethod(nameof(ISharedTwo.Shared)), declarations);
    }

    [Fact]
    public void GetInterfaceDeclarations_ShouldReturnEmpty_WhenMethodHasNoInterfaceMapping()
    {
        var method = typeof(StandaloneClass).GetMethod(nameof(StandaloneClass.NoInterface))!;

        var declarations = method.GetInterfaceDeclarations().ToList();

        Assert.Empty(declarations);
    }

    [Fact]
    public void GetInterfaceDeclarations_ShouldReturnEmpty_WhenTypeImplementsNoInterfaces()
    {
        var method = typeof(StandaloneClass).GetMethod(nameof(StandaloneClass.DoWork))!;

        var declarations = method.GetInterfaceDeclarations().ToList();

        Assert.Empty(declarations);
    }

    [Fact]
    public void GetInterfaceDeclarations_ShouldReturnExplicitImplementation_WhenMethodIsExplicitlyImplemented()
    {
        var explicitMethod = typeof(ExplicitImpl)
            .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Single(m => m.Name.EndsWith("IExplicitTarget.DoWork"));

        var declarations = explicitMethod.GetInterfaceDeclarations().ToList();

        Assert.Single(declarations);
        Assert.Equal(typeof(IExplicitTarget).GetMethod(nameof(IExplicitTarget.DoWork)), declarations[0]);
    }

    [Fact]
    public void GetInterfaceDeclarations_ShouldReturnCorrectDeclaration_ForOverloadedInterfaceMethods()
    {
        var method = typeof(OverloadImpl).GetMethod("Compute", new[] { typeof(int) })!;

        var declarations = method.GetInterfaceDeclarations().ToList();

        Assert.Single(declarations);
        Assert.Equal(typeof(IOverload).GetMethod("Compute", new[] { typeof(int) }), declarations[0]);
    }

    // -------------------------------------------------------------------
    //  IsCovariantReturnMethod
    // -------------------------------------------------------------------

    [Fact]
    public void IsCovariantReturnMethod_ShouldReturnTrue_WhenMethodIsCovariantReturnOverride()
    {
        var method = GetMethod<BaseCovariantReturnService>(nameof(BaseCovariantReturnService.Method), typeof(BaseResult));

        Assert.True(method.IsCovariantReturnMethod());
    }

    [Fact]
    public void IsCovariantReturnMethod_ShouldReturnTrue_ForLeafCovariantReturnMethod()
    {
        var method = GetMethod<LeafCovariantReturnService>(nameof(LeafCovariantReturnService.Method), typeof(LeafResult));

        Assert.True(method.IsCovariantReturnMethod());
    }

    [Fact]
    public void IsCovariantReturnMethod_ShouldReturnFalse_WhenMethodIsOrdinaryOverride()
    {
        var method = GetMethod<OrdinaryOverrideService>(nameof(OrdinaryOverrideService.Method), typeof(object));

        Assert.False(method.IsCovariantReturnMethod());
    }

    [Fact]
    public void IsCovariantReturnMethod_ShouldReturnFalse_WhenMethodIsNotAnOverride()
    {
        var method = GetMethod<CommonService>(nameof(CommonService.Method), typeof(object));

        Assert.False(method.IsCovariantReturnMethod());
    }

    [Fact]
    public void IsCovariantReturnMethod_ShouldReturnFalse_WhenMethodIsOrdinaryOverrideOfCovariantReturn()
    {
        var method = GetMethod<OrdinaryOverrideLeafService>(nameof(OrdinaryOverrideLeafService.Method), typeof(LeafResult));

        Assert.False(method.IsCovariantReturnMethod());
    }

    [Fact]
    public void IsCovariantReturnMethod_ShouldReturnTrue_ForCovariantPropertyGetter()
    {
        var getter = typeof(BaseCovariantReturnService)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Single(p => p.Name == nameof(BaseCovariantReturnService.Property) && p.PropertyType == typeof(BaseResult))
            .GetMethod!;

        Assert.True(getter.IsCovariantReturnMethod());
    }

    // -------------------------------------------------------------------
    //  IsInCovariantReturnChain
    // -------------------------------------------------------------------

    [Fact]
    public void IsInCovariantReturnChain_ShouldReturnTrue_WhenMethodIsItselfCovariantReturn()
    {
        var method = GetMethod<BaseCovariantReturnService>(nameof(BaseCovariantReturnService.Method), typeof(BaseResult));

        Assert.True(method.IsInCovariantReturnChain());
    }

    [Fact]
    public void IsInCovariantReturnChain_ShouldReturnTrue_WhenBaseDefinitionIsCovariantReturn()
    {
        var method = GetMethod<OrdinaryOverrideLeafService>(nameof(OrdinaryOverrideLeafService.Method), typeof(LeafResult));

        Assert.True(method.IsInCovariantReturnChain());
    }

    [Fact]
    public void IsInCovariantReturnChain_ShouldReturnTrue_ForIntermediateCovariantReturnMethod()
    {
        var method = GetMethod<MidCovariantReturnService>(nameof(MidCovariantReturnService.Method), typeof(MidResult));

        Assert.True(method.IsInCovariantReturnChain());
    }

    [Fact]
    public void IsInCovariantReturnChain_ShouldReturnFalse_WhenMethodIsNotInCovariantChain()
    {
        var method = GetMethod<CommonService>(nameof(CommonService.Method), typeof(object));

        Assert.False(method.IsInCovariantReturnChain());
    }

    [Fact]
    public void IsInCovariantReturnChain_ShouldReturnFalse_WhenMethodIsOrdinaryOverrideWithNoCovariantBase()
    {
        var method = GetMethod<OrdinaryOverrideService>(nameof(OrdinaryOverrideService.Method), typeof(object));

        Assert.False(method.IsInCovariantReturnChain());
    }

    [Fact]
    public void IsInCovariantReturnChain_ShouldReturnTrue_ForLeafCovariantPropertyGetter()
    {
        var getter = typeof(LeafCovariantReturnService)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Single(p => p.Name == nameof(LeafCovariantReturnService.Property) && p.PropertyType == typeof(LeafResult))
            .GetMethod!;

        Assert.True(getter.IsInCovariantReturnChain());
    }

    [Fact]
    public void IsInCovariantReturnChain_ShouldReturnTrue_ForOrdinaryOverrideOfCovariantPropertyGetter()
    {
        var getter = typeof(OrdinaryOverrideLeafService)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Single(p => p.Name == nameof(OrdinaryOverrideLeafService.Property) && p.PropertyType == typeof(LeafResult))
            .GetMethod!;

        Assert.True(getter.IsInCovariantReturnChain());
    }

    // -------------------------------------------------------------------
    //  IsSameBaseDefinition
    // -------------------------------------------------------------------

    [Fact]
    public void IsSameBaseDefinition_ShouldReturnTrue_WhenMethodIsComparedWithItself()
    {
        var method = GetMethod<CommonService>(nameof(CommonService.Method), typeof(object));

        Assert.True(method.IsSameBaseDefinition(method));
    }

    [Fact]
    public void IsSameBaseDefinition_ShouldReturnTrue_WhenOverrideSharesBaseDefinitionWithBase()
    {
        var baseMethod = GetMethod<CommonService>(nameof(CommonService.Method), typeof(object));
        var overrideMethod = GetMethod<OrdinaryOverrideService>(nameof(OrdinaryOverrideService.Method), typeof(object));

        Assert.True(baseMethod.IsSameBaseDefinition(overrideMethod));
        Assert.True(overrideMethod.IsSameBaseDefinition(baseMethod));
    }

    [Fact]
    public void IsSameBaseDefinition_ShouldReturnTrue_ForDeepOrdinaryOverrideChain()
    {
        // OrdinaryOverrideService.Method() overrides CommonService.Method()
        // Both have the same base definition: CommonService.Method()
        var baseMethod = GetMethod<CommonService>(nameof(CommonService.Method), typeof(object));
        var overrideMethod = GetMethod<OrdinaryOverrideService>(nameof(OrdinaryOverrideService.Method), typeof(object));

        Assert.True(baseMethod.IsSameBaseDefinition(overrideMethod));
    }

    [Fact]
    public void IsSameBaseDefinition_ShouldReturnTrue_ForCovariantReturnMethodAndItsOrdinaryOverride()
    {
        // OrdinaryOverrideLeafService.Method() overrides LeafCovariantReturnService.Method()
        // Both have the same base definition: LeafCovariantReturnService.Method()
        // (covariant return methods create a new virtual slot, so their base definition points to themselves)
        var covariantMethod = GetMethod<LeafCovariantReturnService>(nameof(LeafCovariantReturnService.Method), typeof(LeafResult));
        var overrideMethod = GetMethod<OrdinaryOverrideLeafService>(nameof(OrdinaryOverrideLeafService.Method), typeof(LeafResult));

        Assert.True(covariantMethod.IsSameBaseDefinition(overrideMethod));
    }

    [Fact]
    public void IsSameBaseDefinition_ShouldReturnFalse_ForCovariantReturnOverrideAndOriginalBase()
    {
        // Covariant return methods create a new virtual slot, so GetBaseDefinition() returns the
        // covariant return method itself, not the original base method.
        var baseMethod = GetMethod<CommonService>(nameof(CommonService.Method), typeof(object));
        var covariantMethod = GetMethod<BaseCovariantReturnService>(nameof(BaseCovariantReturnService.Method), typeof(BaseResult));

        Assert.False(baseMethod.IsSameBaseDefinition(covariantMethod));
    }

    [Fact]
    public void IsSameBaseDefinition_ShouldReturnFalse_ForCovariantReturnChainAndOriginalBase()
    {
        // OrdinaryOverrideLeafService.Method() inherits through a covariant return chain,
        // so its base definition is LeafCovariantReturnService.Method(), not CommonService.Method().
        var baseMethod = GetMethod<CommonService>(nameof(CommonService.Method), typeof(object));
        var leafMethod = GetMethod<OrdinaryOverrideLeafService>(nameof(OrdinaryOverrideLeafService.Method), typeof(LeafResult));

        Assert.False(baseMethod.IsSameBaseDefinition(leafMethod));
    }

    [Fact]
    public void IsSameBaseDefinition_ShouldReturnFalse_WhenMethodsHaveDifferentBaseDefinitions()
    {
        var method = GetMethod<CommonService>(nameof(CommonService.Method), typeof(object));
        var otherMethod = GetMethod<GenericMethodBaseService>(nameof(GenericMethodBaseService.Convert), typeof(BaseResult), parameterCount: 1);

        Assert.False(method.IsSameBaseDefinition(otherMethod));
    }

    [Fact]
    public void IsSameBaseDefinition_ShouldReturnFalse_WhenMethodsAreUnrelatedOverrides()
    {
        var method = GetMethod<OrdinaryOverrideService>(nameof(OrdinaryOverrideService.Method), typeof(object));
        var otherMethod = GetMethod<GenericMethodLeafService>(nameof(GenericMethodLeafService.Convert), typeof(LeafResult), parameterCount: 1);

        Assert.False(method.IsSameBaseDefinition(otherMethod));
    }

    [Fact]
    public void IsSameBaseDefinition_ShouldReturnTrue_ForPropertyGetterOverrideChain()
    {
        var baseGetter = typeof(CommonService).GetProperty(nameof(CommonService.Property))!.GetMethod!;
        var overrideGetter = typeof(OrdinaryOverrideService)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Single(p => p.Name == nameof(OrdinaryOverrideService.Property) && p.PropertyType == typeof(object))
            .GetMethod!;

        Assert.True(baseGetter.IsSameBaseDefinition(overrideGetter));
    }

    [Fact]
    public void IsSameBaseDefinition_ShouldReturnFalse_ForMethodVsPropertyGetter()
    {
        var method = GetMethod<CommonService>(nameof(CommonService.Method), typeof(object));
        var getter = typeof(CommonService).GetProperty(nameof(CommonService.Property))!.GetMethod!;

        Assert.False(method.IsSameBaseDefinition(getter));
    }

    // -------------------------------------------------------------------
    //  IsConstructedGenericMethod
    // -------------------------------------------------------------------

    [Fact]
    public void IsConstructedGenericMethod_ShouldReturnFalse_ForNonGenericMethod()
    {
        var method = GetMethod<CommonService>(nameof(CommonService.Method), typeof(object));

        Assert.False(method.IsConstructedGenericMethod());
    }

    [Fact]
    public void IsConstructedGenericMethod_ShouldReturnFalse_ForGenericMethodDefinition()
    {
        var method = typeof(GenericMethodClass)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(m => m.Name == nameof(GenericMethodClass.GenericMethod) && m.IsGenericMethodDefinition);

        Assert.False(method.IsConstructedGenericMethod());
    }

    [Fact]
    public void IsConstructedGenericMethod_ShouldReturnTrue_ForConstructedGenericMethod()
    {
        var definition = typeof(GenericMethodClass)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(m => m.Name == nameof(GenericMethodClass.GenericMethod) && m.IsGenericMethodDefinition);

        var constructed = definition.MakeGenericMethod(typeof(int));

        Assert.True(constructed.IsConstructedGenericMethod());
    }

    [Fact]
    public void IsConstructedGenericMethod_ShouldReturnTrue_ForConstructedGenericMethodWithMultipleTypeArgs()
    {
        var definition = typeof(GenericMethodClass)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(m => m.Name == nameof(GenericMethodClass.MultiGeneric) && m.IsGenericMethodDefinition);

        var constructed = definition.MakeGenericMethod(typeof(int), typeof(string));

        Assert.True(constructed.IsConstructedGenericMethod());
    }

    [Fact]
    public void IsConstructedGenericMethod_ShouldReturnFalse_ForGenericMethodWithParam()
    {
        var method = typeof(GenericMethodClass)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(m => m.Name == nameof(GenericMethodClass.GenericMethodWithParam) && m.IsGenericMethodDefinition);

        Assert.False(method.IsConstructedGenericMethod());
    }

    [Fact]
    public void IsConstructedGenericMethod_ShouldReturnTrue_ForConstructedGenericMethodFromBaseService()
    {
        var definition = GetMethod<GenericMethodBaseService>(nameof(GenericMethodBaseService.Convert), typeof(BaseResult), parameterCount: 1);

        // The definition from the base service is a generic method definition
        Assert.False(definition.IsConstructedGenericMethod());

        var constructed = definition.MakeGenericMethod(typeof(string));
        Assert.True(constructed.IsConstructedGenericMethod());
    }

    [Fact]
    public void IsConstructedGenericMethod_ShouldReturnFalse_ForPropertyGetter()
    {
        var getter = typeof(CommonService).GetProperty(nameof(CommonService.Property))!.GetMethod!;

        Assert.False(getter.IsConstructedGenericMethod());
    }

    // -------------------------------------------------------------------
    //  Helper methods
    // -------------------------------------------------------------------

    private static MethodInfo GetMethod<T>(string name, Type returnType, int parameterCount = 0)
    {
        return GetMethod(typeof(T), name, returnType, parameterCount);
    }

    private static MethodInfo GetMethod(Type type, string name, Type returnType, int parameterCount = 0)
    {
        return type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(method =>
                method.Name == name
                && method.ReturnType == returnType
                && method.GetParameters().Length == parameterCount);
    }

    // -------------------------------------------------------------------
    //  Test types
    // -------------------------------------------------------------------

    public interface ISharedOne
    {
        void Shared();
    }

    public interface ISharedTwo
    {
        void Shared();
    }

    public class MultiInterfaceImpl : ISharedOne, ISharedTwo
    {
        public void Shared() { }
    }

    public interface IExplicitTarget
    {
        void DoWork();
    }

    public class ExplicitImpl : IExplicitTarget
    {
        void IExplicitTarget.DoWork() { }
    }

    public interface IOverload
    {
        int Compute(int value);
        int Compute(string value);
    }

    public class OverloadImpl : IOverload
    {
        public int Compute(int value) => value;
        public int Compute(string value) => value.Length;
    }

    public class StandaloneClass
    {
        public void DoWork() { }
        public void NoInterface() { }
    }

    public class GenericMethodClass
    {
        public T GenericMethod<T>() => default!;

        public T GenericMethodWithParam<T>(T value) => value;

        public void NonGenericMethod() { }

        public TResult MultiGeneric<TInput, TResult>(TInput input) => default!;
    }
}
