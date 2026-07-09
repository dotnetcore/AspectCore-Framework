using AspectCore.DynamicProxy;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;
using Xunit.Abstractions;

namespace AspectCore.Tests.DynamicProxy;

public partial class CovariantReturnTypeTests(ITestOutputHelper output) : DynamicProxyTestBase
{
    /// <summary>
    /// Verifies that an object is exactly the given type (and not a derived type), and that it satisfies the given predicate.
    /// </summary>
    private static void AssertTypeValue<T>(object value, Action<T> action)
    {
        var v = Assert.IsType<T>(value);
        action(v);
    }

    private static void AssertEqual<T>(object expected, object actual, Action<T, T> action)
    {
        var e = Assert.IsType<T>(expected);
        var a = Assert.IsType<T>(actual);
        action(e, a);
    }

    public readonly record struct EnumFlag<TEnum>(TEnum Value, string Name) where TEnum : struct, Enum;

    public static IEnumerable<EnumFlag<TEnum>> GetAtomicFlags<TEnum>(TEnum value, params string[] preferredNames) where TEnum : struct, Enum
    {
        var rawValue = Convert.ToUInt64(value);

        var preferredByRawValue = typeof(TEnum)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => preferredNames.Contains(f.Name))
            .ToDictionary(
                f => Convert.ToUInt64(f.GetValue(null)!),
                f => new EnumFlag<TEnum>((TEnum)f.GetValue(null)!, f.Name));

        var seen = new HashSet<ulong>();

        foreach (var field in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var flag = (TEnum)field.GetValue(null)!;
            var rawFlag = Convert.ToUInt64(flag);

            if (rawFlag == 0)
                continue;

            if ((rawFlag & (rawFlag - 1)) != 0)
                continue;

            if ((rawValue & rawFlag) != rawFlag)
                continue;

            if (!seen.Add(rawFlag))
                continue;

            if (preferredByRawValue.TryGetValue(rawFlag, out var preferred))
                yield return preferred;
            else
                yield return new EnumFlag<TEnum>(flag, field.Name);
        }
    }

    [Fact]
    public void Print()
    {
        var service = new DerivedLeafCovariantReturnService();
        var proxy = ProxyGenerator.CreateClassProxy<LeafCovariantReturnService>();

        var skip = new[] { nameof(ToString), nameof(GetType), nameof(object.Equals), nameof(GetHashCode) };

        foreach (var obj in new[] { service, proxy })
        {
            var type = obj.GetType();
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            output.WriteLine($"{type.FullName}'s methods:");
            foreach (var method in methods)
            {
                if (method.IsDefined(typeof(CompilerGeneratedAttribute)))
                    continue;

                if (skip.Contains(method.Name))
                    continue;

                var attrs = method.GetCustomAttributesData().Select(m => m.AttributeType.Name);
                var flags = GetAtomicFlags(method.Attributes, nameof(MethodAttributes.NewSlot)).Select(m => m.Name.ToString());

                output.WriteLine($"[{method.Name}]DeclaringType: {method.DeclaringType?.Name}, ReturnType: {method.ReturnType.Name}, Attributes: {string.Join(", ", attrs)}, MethodAttributes: {string.Join(", ", flags)}");
            }
            output.WriteLine("");
        }

        Assert.Equal(service.Property.Name, proxy.Property.Name);
        Assert.Equal(service.Method().Name, proxy.Method().Name);
        Assert.Equal(service.InterceptedProperty.Name + nameof(ReturnTypeInterceptor), proxy.InterceptedProperty.Name);
        Assert.Equal(service.InterceptedMethod().Name + nameof(ReturnTypeInterceptor), proxy.InterceptedMethod().Name);

        var serviceInter = (ICommonService)service;
        var proxyInter = (ICommonService)proxy;

        AssertEqual<LeafResult>(serviceInter.Property, proxyInter.Property, (e, a) => Assert.Equal(e.Name, a.Name));
        AssertEqual<LeafResult>(serviceInter.Method(), proxyInter.Method(), (e, a) => Assert.Equal(e.Name, a.Name));
        AssertEqual<LeafResult>(serviceInter.InterceptedProperty, proxyInter.InterceptedProperty, (e, a) => Assert.Equal(e.Name + nameof(ReturnTypeInterceptor), a.Name));
        AssertEqual<LeafResult>(serviceInter.InterceptedMethod(), proxyInter.InterceptedMethod(), (e, a) => Assert.Equal(e.Name + nameof(ReturnTypeInterceptor), a.Name));
    }

    [Fact]
    public void CreateClassProxy_ForCovariantReturnType_ShouldUseStringReturnType()
    {
        var service = ProxyGenerator.CreateClassProxy<BaseCovariantReturnService>();
        AssertTypeValue<BaseResult>(service.Method(), v => Assert.Equal(nameof(BaseCovariantReturnService), v.Name));
        AssertTypeValue<BaseResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(BaseCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<BaseResult>(service.Property, v => Assert.Equal(nameof(BaseCovariantReturnService), v.Name));
        AssertTypeValue<BaseResult>(service.InterceptedProperty, v => Assert.Equal(nameof(BaseCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForDerivedCovariantReturnType_ShouldUseOverriddenInterceptedMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<MidCovariantReturnService>();
        AssertTypeValue<MidResult>(service.Property, v => Assert.Equal(nameof(MidCovariantReturnService), v.Name));
        AssertTypeValue<MidResult>(service.Method(), v => Assert.Equal(nameof(MidCovariantReturnService), v.Name));
        AssertTypeValue<MidResult>(service.InterceptedProperty, v => Assert.Equal(nameof(MidCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<MidResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(MidCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForLeafCovariantReturnType_ShouldUseLeafInterceptedMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<LeafCovariantReturnService>();
        AssertTypeValue<LeafResult>(service.Property, v => Assert.Equal(nameof(LeafCovariantReturnService), v.Name));
        AssertTypeValue<LeafResult>(service.Method(), v => Assert.Equal(nameof(LeafCovariantReturnService), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedProperty, v => Assert.Equal(nameof(LeafCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(LeafCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForLeafCovariantReturnTypeAndInterfaceView_ShouldUseLeafMethodAndProperty()
    {
        var service = Assert.IsAssignableFrom<ICommonService>(ProxyGenerator.CreateClassProxy<LeafCovariantReturnService>());

        AssertTypeValue<LeafResult>(service.Property, v => Assert.Equal(nameof(LeafCovariantReturnService), v.Name));
        AssertTypeValue<LeafResult>(service.Method(), v => Assert.Equal(nameof(LeafCovariantReturnService), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedProperty, v => Assert.Equal(nameof(LeafCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(LeafCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForLeafCovariantReturnTypeAndBaseClassView_ShouldUseLeafMethodAndProperty()
    {
        var service = Assert.IsAssignableFrom<MidCovariantReturnService>(ProxyGenerator.CreateClassProxy<LeafCovariantReturnService>());

        AssertTypeValue<LeafResult>(service.Property, v => Assert.Equal(nameof(LeafCovariantReturnService), v.Name));
        AssertTypeValue<LeafResult>(service.Method(), v => Assert.Equal(nameof(LeafCovariantReturnService), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedProperty, v => Assert.Equal(nameof(LeafCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(LeafCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForOrdinaryOverrideAfterCovariantReturnChain_ShouldUseOrdinaryOverrideMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<OrdinaryOverrideService>();
        AssertTypeValue<LeafResult>(service.Property, v => Assert.Equal(nameof(OrdinaryOverrideService), v.Name));
        AssertTypeValue<LeafResult>(service.Method(), v => Assert.Equal(nameof(OrdinaryOverrideService), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedProperty, v => Assert.Equal(nameof(OrdinaryOverrideService) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(OrdinaryOverrideService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForDerivedOrdinaryOverrideAfterCovariantReturnChain_ShouldUseInheritedOrdinaryOverrideMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<DerivedOrdinaryOverrideService>();
        AssertTypeValue<LeafResult>(service.Property, v => Assert.Equal(nameof(OrdinaryOverrideService), v.Name));
        AssertTypeValue<LeafResult>(service.Method(), v => Assert.Equal(nameof(OrdinaryOverrideService), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedProperty, v => Assert.Equal(nameof(OrdinaryOverrideService) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(OrdinaryOverrideService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForBaseServiceAndCovariantImplementation_ShouldUseStringReturnType()
    {
        var service = ProxyGenerator.CreateClassProxy<CommonService, BaseCovariantReturnService>();
        AssertTypeValue<BaseResult>(service.Property, v => Assert.Equal(nameof(BaseCovariantReturnService), v.Name));
        AssertTypeValue<BaseResult>(service.Method(), v => Assert.Equal(nameof(BaseCovariantReturnService), v.Name));
        AssertTypeValue<BaseResult>(service.InterceptedProperty, v => Assert.Equal(nameof(BaseCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<BaseResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(BaseCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForBaseServiceAndDerivedImplementation_ShouldUseDerivedInterceptedMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<CommonService, MidCovariantReturnService>();
        AssertTypeValue<MidResult>(service.Property, v => Assert.Equal(nameof(MidCovariantReturnService), v.Name));
        AssertTypeValue<MidResult>(service.Method(), v => Assert.Equal(nameof(MidCovariantReturnService), v.Name));
        AssertTypeValue<MidResult>(service.InterceptedProperty, v => Assert.Equal(nameof(MidCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<MidResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(MidCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForCovariantServiceAndDerivedImplementation_ShouldUseDerivedInterceptedMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<BaseCovariantReturnService, MidCovariantReturnService>();
        AssertTypeValue<MidResult>(service.Method(), v => Assert.Equal(nameof(MidCovariantReturnService), v.Name));
        AssertTypeValue<MidResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(MidCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<MidResult>(service.Property, v => Assert.Equal(nameof(MidCovariantReturnService), v.Name));
        AssertTypeValue<MidResult>(service.InterceptedProperty, v => Assert.Equal(nameof(MidCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForCovariantServiceAndLeafImplementation_ShouldUseLeafInterceptedMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<BaseCovariantReturnService, LeafCovariantReturnService>();
        AssertTypeValue<LeafResult>(service.Method(), v => Assert.Equal(nameof(LeafCovariantReturnService), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(LeafCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<LeafResult>(service.Property, v => Assert.Equal(nameof(LeafCovariantReturnService), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedProperty, v => Assert.Equal(nameof(LeafCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForLeafServiceAndOrdinaryOverrideImplementation_ShouldUseOrdinaryOverrideMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<LeafCovariantReturnService, OrdinaryOverrideService>();
        AssertTypeValue<LeafResult>(service.Method(), v => Assert.Equal(nameof(OrdinaryOverrideService), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(OrdinaryOverrideService) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<LeafResult>(service.Property, v => Assert.Equal(nameof(OrdinaryOverrideService), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedProperty, v => Assert.Equal(nameof(OrdinaryOverrideService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateInterfaceProxy_ForBaseInterfaceAndCovariantImplementation_ShouldUseStringReturnType()
    {
        var service = ProxyGenerator.CreateInterfaceProxy<ICommonService, BaseCovariantReturnService>();
        AssertTypeValue<BaseResult>(service.Property, v => Assert.Equal(nameof(BaseCovariantReturnService), v.Name));
        AssertTypeValue<BaseResult>(service.Method(), v => Assert.Equal(nameof(BaseCovariantReturnService), v.Name));
        AssertTypeValue<BaseResult>(service.InterceptedProperty, v => Assert.Equal(nameof(BaseCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<BaseResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(BaseCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateInterfaceProxy_ForBaseInterfaceAndDerivedImplementation_ShouldUseDerivedInterceptedMembers()
    {
        var service = ProxyGenerator.CreateInterfaceProxy<ICommonService, MidCovariantReturnService>();
        AssertTypeValue<MidResult>(service.Property, v => Assert.Equal(nameof(MidCovariantReturnService), v.Name));
        AssertTypeValue<MidResult>(service.Method(), v => Assert.Equal(nameof(MidCovariantReturnService), v.Name));
        AssertTypeValue<MidResult>(service.InterceptedProperty, v => Assert.Equal(nameof(MidCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<MidResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(MidCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateInterfaceProxy_ForBaseInterfaceAndLeafImplementation_ShouldUseLeafInterceptedMembers()
    {
        var service = ProxyGenerator.CreateInterfaceProxy<ICommonService, LeafCovariantReturnService>();
        AssertTypeValue<LeafResult>(service.Property, v => Assert.Equal(nameof(LeafCovariantReturnService), v.Name));
        AssertTypeValue<LeafResult>(service.Method(), v => Assert.Equal(nameof(LeafCovariantReturnService), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedProperty, v => Assert.Equal(nameof(LeafCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(LeafCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateInterfaceProxy_ForBaseInterfaceAndOrdinaryOverrideImplementation_ShouldUseOrdinaryOverrideMembers()
    {
        var service = ProxyGenerator.CreateInterfaceProxy<ICommonService, OrdinaryOverrideService>();
        AssertTypeValue<LeafResult>(service.Property, v => Assert.Equal(nameof(OrdinaryOverrideService), v.Name));
        AssertTypeValue<LeafResult>(service.Method(), v => Assert.Equal(nameof(OrdinaryOverrideService), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedProperty, v => Assert.Equal(nameof(OrdinaryOverrideService) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(OrdinaryOverrideService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForMethodOnlyOrdinaryOverrideAfterCovariantReturnChain_ShouldUseOrdinaryOverrideMethod()
    {
        var service = ProxyGenerator.CreateClassProxy<MethodOnlyOrdinaryOverrideService>();
        AssertTypeValue<LeafResult>(service.Method(), v => Assert.Equal(nameof(MethodOnlyOrdinaryOverrideService), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(MethodOnlyOrdinaryOverrideService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForPropertyOnlyOrdinaryOverrideAfterCovariantReturnChain_ShouldUseOrdinaryOverrideProperty()
    {
        var service = ProxyGenerator.CreateClassProxy<PropertyOnlyOrdinaryOverrideService>();
        AssertTypeValue<LeafResult>(service.Property, v => Assert.Equal(nameof(PropertyOnlyOrdinaryOverrideService), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedProperty, v => Assert.Equal(nameof(PropertyOnlyOrdinaryOverrideService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForInvariantGenericReturnHiddenMembers_ShouldNotTreatMembersAsCovariantOverrides()
    {
        var service = ProxyGenerator.CreateClassProxy<InvariantGenericReturnLeafService>();

        Assert.IsType<List<LeafResult>>(service.Create());
        Assert.IsType<List<LeafResult>>(service.Items);
    }

    [Fact]
    public void CreateInterfaceProxy_ForCovariantInterface_ShouldUseLeafInterfaceMembers()
    {
        var service = ProxyGenerator.CreateInterfaceProxy<ICovariantInterfaceLeafService, CovariantInterfaceLeafImplementation>();

        AssertTypeValue<LeafResult>(service.Property, v => Assert.Equal(nameof(CovariantInterfaceLeafImplementation), v.Name));
        AssertTypeValue<LeafResult>(service.Method(), v => Assert.Equal(nameof(CovariantInterfaceLeafImplementation), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedProperty, v => Assert.Equal(nameof(CovariantInterfaceLeafImplementation) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(CovariantInterfaceLeafImplementation) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateInterfaceProxy_ForBaseCovariantInterfaceAndLeafImplementation_ShouldUseLeafImplementationMembers()
    {
        var service = ProxyGenerator.CreateInterfaceProxy<ICovariantInterfaceBaseService, CovariantInterfaceLeafImplementation>();

        AssertTypeValue<LeafResult>(service.Property, v => Assert.Equal(nameof(CovariantInterfaceLeafImplementation), v.Name));
        AssertTypeValue<LeafResult>(service.Method(), v => Assert.Equal(nameof(CovariantInterfaceLeafImplementation), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedProperty, v => Assert.Equal(nameof(CovariantInterfaceLeafImplementation) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(CovariantInterfaceLeafImplementation) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForIndexerCovariantReturn_ShouldUseLeafIndexerGetter()
    {
        var service = ProxyGenerator.CreateClassProxy<IndexerLeafCovariantReturnService>();

        AssertTypeValue<LeafResult>(service[0], v => Assert.Equal(nameof(IndexerLeafCovariantReturnService), v.Name));
        AssertTypeValue<LeafResult>(service["key"], v => Assert.Equal(nameof(IndexerLeafCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
    }
}
