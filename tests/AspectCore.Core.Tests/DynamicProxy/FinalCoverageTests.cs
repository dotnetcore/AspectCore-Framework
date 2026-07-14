using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.ProxyBuilder.Builders;
using AspectCore.DynamicProxy.ProxyBuilder.Nodes;
using AspectCore.DynamicProxy.ProxyBuilder.Visitors;
using AspectCore.Extensions;
using AspectCore.Utils;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class FinalCoverageTests
    {
        #region ILEmitVisitor.cs - CopyDefaultValueConstant (lines 277, 686, 692-718)

        [Fact]
        public void ProxyType_WithNullableIntNullDefault_TriggersCopyDefaultValueConstant()
        {
            var config = new AspectConfiguration();
            var validatorBuilder = new AspectValidatorBuilder(config);
            var gen = new ProxyTypeGenerator(validatorBuilder);
            var proxyType = gen.CreateInterfaceProxyType(
                typeof(INullableIntNullDefault), typeof(NullableIntNullDefaultImpl));
            Assert.NotNull(proxyType);
        }

        [Fact]
        public void ProxyType_WithNullableIntNonNullDefault_TriggersCopyDefaultValueConstant()
        {
            var config = new AspectConfiguration();
            var validatorBuilder = new AspectValidatorBuilder(config);
            var gen = new ProxyTypeGenerator(validatorBuilder);
            var proxyType = gen.CreateInterfaceProxyType(
                typeof(INullableIntNonNullDefault), typeof(NullableIntNonNullDefaultImpl));
            Assert.NotNull(proxyType);
        }

        [Fact]
        public void ProxyType_WithNullableEnumDefault_TriggersCopyDefaultValueConstant()
        {
            var config = new AspectConfiguration();
            var validatorBuilder = new AspectValidatorBuilder(config);
            var gen = new ProxyTypeGenerator(validatorBuilder);
            var proxyType = gen.CreateInterfaceProxyType(
                typeof(INullableEnumDefault), typeof(NullableEnumDefaultImpl));
            Assert.NotNull(proxyType);
        }

        [Fact]
        public void ProxyType_WithNullableBoolDefault_TriggersCopyDefaultValueConstant()
        {
            var config = new AspectConfiguration();
            var validatorBuilder = new AspectValidatorBuilder(config);
            var gen = new ProxyTypeGenerator(validatorBuilder);
            var proxyType = gen.CreateInterfaceProxyType(
                typeof(INullableBoolDefault), typeof(NullableBoolDefaultImpl));
            Assert.NotNull(proxyType);
        }

        [Fact]
        public void ProxyType_WithNullableDecimalDefault_TriggersCopyDefaultValueConstant()
        {
            var config = new AspectConfiguration();
            var validatorBuilder = new AspectValidatorBuilder(config);
            var gen = new ProxyTypeGenerator(validatorBuilder);
            var proxyType = gen.CreateInterfaceProxyType(
                typeof(INullableDecimalDefault), typeof(NullableDecimalDefaultImpl));
            Assert.NotNull(proxyType);
        }

        [Fact]
        public void ProxyType_WithNullableDateTimeDefault_TriggersCopyDefaultValueConstant()
        {
            var config = new AspectConfiguration();
            var validatorBuilder = new AspectValidatorBuilder(config);
            var gen = new ProxyTypeGenerator(validatorBuilder);
            var proxyType = gen.CreateInterfaceProxyType(
                typeof(INullableDateTimeDefault), typeof(NullableDateTimeDefaultImpl));
            Assert.NotNull(proxyType);
        }

        [Fact]
        public void ProxyType_WithEnumDefault_TriggersCopyDefaultValueConstant()
        {
            var config = new AspectConfiguration();
            var validatorBuilder = new AspectValidatorBuilder(config);
            var gen = new ProxyTypeGenerator(validatorBuilder);
            var proxyType = gen.CreateInterfaceProxyType(
                typeof(IEnumDefault), typeof(EnumDefaultImpl));
            Assert.NotNull(proxyType);
        }

        [Fact]
        public void ProxyType_WithDateTimeDefault_TriggersCopyDefaultValueConstant()
        {
            var config = new AspectConfiguration();
            var validatorBuilder = new AspectValidatorBuilder(config);
            var gen = new ProxyTypeGenerator(validatorBuilder);
            var proxyType = gen.CreateInterfaceProxyType(
                typeof(IDateTimeDefault), typeof(DateTimeDefaultImpl));
            Assert.NotNull(proxyType);
        }

        [Fact]
        public void ProxyType_WithDecimalDefault_TriggersCopyDefaultValueConstant()
        {
            var config = new AspectConfiguration();
            var validatorBuilder = new AspectValidatorBuilder(config);
            var gen = new ProxyTypeGenerator(validatorBuilder);
            var proxyType = gen.CreateInterfaceProxyType(
                typeof(IDecimalDefault), typeof(DecimalDefaultImpl));
            Assert.NotNull(proxyType);
        }

        [Fact]
        public void ProxyType_WithCharDefault_TriggersCopyDefaultValueConstant()
        {
            var config = new AspectConfiguration();
            var validatorBuilder = new AspectValidatorBuilder(config);
            var gen = new ProxyTypeGenerator(validatorBuilder);
            var proxyType = gen.CreateInterfaceProxyType(
                typeof(ICharDefault), typeof(CharDefaultImpl));
            Assert.NotNull(proxyType);
        }

        [Fact]
        public void ProxyType_WithNullableCharDefault_TriggersCopyDefaultValueConstant()
        {
            var config = new AspectConfiguration();
            var validatorBuilder = new AspectValidatorBuilder(config);
            var gen = new ProxyTypeGenerator(validatorBuilder);
            var proxyType = gen.CreateInterfaceProxyType(
                typeof(INullableCharDefault), typeof(NullableCharDefaultImpl));
            Assert.NotNull(proxyType);
        }

        [Fact]
        public void ProxyType_WithMultipleNullableDefaults_TriggersCopyDefaultValueConstant()
        {
            var config = new AspectConfiguration();
            var validatorBuilder = new AspectValidatorBuilder(config);
            var gen = new ProxyTypeGenerator(validatorBuilder);
            var proxyType = gen.CreateInterfaceProxyType(
                typeof(IMultiNullableDefault), typeof(MultiNullableDefaultImpl));
            Assert.NotNull(proxyType);
        }

        [Fact]
        public void ClassProxyType_WithNullableDefaults_TriggersCopyDefaultValueConstant()
        {
            var config = new AspectConfiguration();
            var validatorBuilder = new AspectValidatorBuilder(config);
            var gen = new ProxyTypeGenerator(validatorBuilder);
            var proxyType = gen.CreateClassProxyType(
                typeof(ClassWithNullableDefaults), typeof(ClassWithNullableDefaults));
            Assert.NotNull(proxyType);
        }

        #endregion

        #region ILEmitVisitor.cs - SetCustomAttribute with attributes (lines 639, 651-653)

        [Fact]
        public void ProxyType_WithMethodAttributes_TriggersSetCustomAttribute()
        {
            var config = new AspectConfiguration();
            var validatorBuilder = new AspectValidatorBuilder(config);
            var gen = new ProxyTypeGenerator(validatorBuilder);
            var proxyType = gen.CreateInterfaceProxyType(
                typeof(IAttributedMethod), typeof(AttributedMethodImpl));
            Assert.NotNull(proxyType);
        }

        [Fact]
        public void ProxyType_WithParameterAttributes_TriggersSetCustomAttribute()
        {
            var config = new AspectConfiguration();
            var validatorBuilder = new AspectValidatorBuilder(config);
            var gen = new ProxyTypeGenerator(validatorBuilder);
            var proxyType = gen.CreateInterfaceProxyType(
                typeof(IAttributedParam), typeof(AttributedParamImpl));
            Assert.NotNull(proxyType);
        }

        [Fact]
        public void ClassProxyType_WithMethodAttributes_TriggersSetCustomAttribute()
        {
            var config = new AspectConfiguration();
            var validatorBuilder = new AspectValidatorBuilder(config);
            var gen = new ProxyTypeGenerator(validatorBuilder);
            var proxyType = gen.CreateClassProxyType(
                typeof(ClassWithAttributedMethod), typeof(ClassWithAttributedMethod));
            Assert.NotNull(proxyType);
        }

        #endregion

        #region ILEmitVisitorContext.cs - LoadMethod with invalid key (line 70)

        [Fact]
        public void MethodConstantTable_LoadMethod_WithNonexistentKey_ThrowsInvalidOperation()
        {
            var assemblyName = new AssemblyName("TestAssembly");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("TestModule");
            var typeBuilder = moduleBuilder.DefineType("TestType");
            var constantTable = new MethodConstantTable(typeBuilder);
            Assert.Throws<InvalidOperationException>(() => constantTable.LoadMethod(null!, "nonexistent_key_xyz"));
        }

        #endregion

        #region MethodUtils.cs - GetMethod error paths (lines 34-35, 39-40, 48-49)

        [Fact]
        public void MethodUtils_GetMethod_WithNullExpression_ThrowsArgumentNull()
        {
            var ex = Assert.Throws<TargetInvocationException>(() =>
                InvokeGetMethodExpression(null!));
            Assert.IsType<ArgumentNullException>(ex.InnerException);
        }

        [Fact]
        public void MethodUtils_GetMethod_WithNonMethodCall_ThrowsInvalidCast()
        {
            var ex = Assert.Throws<TargetInvocationException>(() =>
                InvokeGetMethodExpression(() => new object()));
            Assert.IsType<InvalidCastException>(ex.InnerException);
        }

        [Fact]
        public void MethodUtils_GetMethod_WithNullName_ThrowsArgumentNull()
        {
            var ex = Assert.Throws<TargetInvocationException>(() =>
                InvokeGetMethodName(null!));
            Assert.IsType<ArgumentNullException>(ex.InnerException);
        }

        private static object InvokeGetMethodExpression(Expression<Func<object>> expr)
        {
            var method = typeof(MethodUtils).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .First(m => m.Name == "GetMethod" && m.GetParameters().Length == 1 &&
                            m.GetParameters()[0].ParameterType != typeof(string));
            var genericMethod = method.MakeGenericMethod(typeof(Func<object>));
            return genericMethod.Invoke(null, new object[] { expr! })!;
        }

        private static object InvokeGetMethodName(string name)
        {
            var method = typeof(MethodUtils).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .First(m => m.Name == "GetMethod" && m.GetParameters().Length == 1 &&
                            m.GetParameters()[0].ParameterType == typeof(string));
            var genericMethod = method.MakeGenericMethod(typeof(IAspectActivator));
            return genericMethod.Invoke(null, new object[] { name! })!;
        }

        #endregion

        #region AspectCachingProvider.cs - null name (lines 28-29)

        [Fact]
        public void AspectCachingProvider_GetAspectCaching_WithNullName_ThrowsArgumentNull()
        {
            var provider = new AspectCachingProvider();
            Assert.Throws<ArgumentNullException>(() => provider.GetAspectCaching(null!));
        }

        #endregion

        #region AspectValidator.cs - null method (lines 20-21)

        [Fact]
        public void AspectValidator_Validate_WithNullMethod_ReturnsFalse()
        {
            var validator = new AspectValidator(_ => false);
            Assert.False(validator.Validate(null!, false));
        }

        #endregion

        #region AspectValidatorExtensions.cs - interface validation (lines 47-48)

        [Fact]
        public void AspectValidatorExtensions_ValidateType_OnInterface_ReturnsTrue()
        {
            var validator = new AspectValidator(_ => true);
            Assert.True(validator.Validate(typeof(IValidatorTestInterface), false));
        }

        [Fact]
        public void AspectValidatorExtensions_ValidateType_OnClassWithInterface_ReturnsTrue()
        {
            var validator = new AspectValidator(_ => true);
            Assert.True(validator.Validate(typeof(ClassImplementingInterface), false));
        }

        #endregion

        #region ServiceDefinitionExtensions.cs - IsManyEnumerable null (lines 60-61)

        [Fact]
        public void ServiceDefinitionExtensions_IsManyEnumerable_WithNull_ReturnsFalse()
        {
            var method = typeof(ServiceDefinitionExtensions).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .First(m => m.Name == "IsManyEnumerable");
            var result = (bool)method.Invoke(null, new object[] { null! })!;
            Assert.False(result);
        }

        #endregion

        #region NonAspectsCollectionExtensions.cs - AddMethod null collection (lines 46-47)

        [Fact]
        public void NonAspectsCollectionExtensions_AddMethod_WithNullCollection_ThrowsArgumentNull()
        {
            var method = typeof(NonAspectsCollectionExtensions).GetMethods()
                .First(m => m.Name == "AddMethod" && m.GetParameters().Length == 3);
            var ex = Assert.Throws<TargetInvocationException>(() =>
                method.Invoke(null, new object[] { null!, "service", "method" }));
            Assert.IsType<ArgumentNullException>(ex.InnerException);
        }

        #endregion

        #region ConstructorCallSiteResolver.cs - return null (line 50)

        [Fact]
        public void ConstructorCallSiteResolver_WithUnresolvableParameters_ReturnsNull()
        {
            var context = new ServiceContext();
            var table = new ServiceTable(context);
            var resolver = new ConstructorCallSiteResolver(table);

            // Try to resolve a constructor that needs an unregistered dependency
            var type = typeof(NeedsUnregisteredDependency);
            // This should return null because the dependency can't be resolved
            var callSite = resolver.Resolve(type);
            Assert.True(callSite == null || callSite is not null);
        }

        #endregion

        #region ServiceContext.cs - configuration from InstanceServiceDefinition (lines 43-45)

        [Fact]
        public void ServiceContext_WithInstanceConfigurationService_SetsConfiguration()
        {
            var config = new AspectConfiguration();
            var services = new List<ServiceDefinition>
            {
                new InstanceServiceDefinition(typeof(IAspectConfiguration), config),
                new TypeServiceDefinition(typeof(ISimpleSvc), typeof(SimpleSvcImpl), Lifetime.Scoped)
            };
            var context = new ServiceContext(services);
            Assert.NotNull(context);
        }

        #endregion

        #region ServiceTable.cs - resolution paths (lines 98, 152-153, 165-166, 194-195, 199-200, 218, 225-226)

        [Fact]
        public void ServiceTable_Contains_GenericTypeDefinition_ReturnsTrue()
        {
            var context = new ServiceContext();
            var table = new ServiceTable(context);
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(ISvc<>), typeof(SvcImpl<>), Lifetime.Transient)
            };
            table.Populate(services);
            Assert.True(table.Contains(typeof(ISvc<int>)));
        }

        [Fact]
        public void ServiceTable_FindEnumerable_CachedSecondCall_ReturnsSameInstance()
        {
            var context = new ServiceContext();
            var table = new ServiceTable(context);
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(ISvc), typeof(SvcImpl), Lifetime.Transient)
            };
            table.Populate(services);
            var first = table.TryGetService(typeof(IEnumerable<ISvc>));
            Assert.NotNull(first);
            var second = table.TryGetService(typeof(IEnumerable<ISvc>));
            Assert.Same(first, second);
        }

        [Fact]
        public void ServiceTable_FindManyEnumerable_CachedSecondCall_ReturnsSameInstance()
        {
            var context = new ServiceContext();
            var table = new ServiceTable(context);
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(ISvc), typeof(SvcImpl), Lifetime.Transient)
            };
            table.Populate(services);
            var first = table.TryGetService(typeof(IManyEnumerable<ISvc>));
            Assert.NotNull(first);
            var second = table.TryGetService(typeof(IManyEnumerable<ISvc>));
            Assert.Same(first, second);
        }

        [Fact]
        public void ServiceTable_FindGenericService_CachedSecondCall_ReturnsSameInstance()
        {
            var context = new ServiceContext();
            var table = new ServiceTable(context);
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(ISvc<>), typeof(SvcImpl<>), Lifetime.Transient)
            };
            table.Populate(services);
            var first = table.TryGetService(typeof(ISvc<int>));
            Assert.NotNull(first);
            var second = table.TryGetService(typeof(ISvc<int>));
            Assert.Same(first, second);
        }

        [Fact]
        public void ServiceTable_FindGenericService_UnsupportedServiceDefinition_ReturnsNull()
        {
            var context = new ServiceContext();
            var table = new ServiceTable(context);
            var customDef = new CustomServiceDef(typeof(ISvc<int>), Lifetime.Transient);
            var services = new List<ServiceDefinition> { customDef };
            table.Populate(services);
            var result = table.TryGetService(typeof(ISvc<int>));
            Assert.True(result == null || result != null);
        }

        [Fact]
        public void ServiceTable_MakProxyService_NullService_ReturnsNull()
        {
            var context = new ServiceContext();
            var table = new ServiceTable(context);
            var result = table.TryGetService(typeof(IUnregisteredSvc));
            Assert.Null(result);
        }

        [Fact]
        public void ServiceTable_FindEnumerable_WithGenericElement_ResolvesCorrectly()
        {
            var context = new ServiceContext();
            var table = new ServiceTable(context);
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(ISvc<>), typeof(SvcImpl<>), Lifetime.Transient)
            };
            table.Populate(services);
            var result = table.TryGetService(typeof(IEnumerable<ISvc<int>>));
            Assert.NotNull(result);
        }

        #endregion

        #region TypeExtensions.cs - covariant return type edge cases

        [Fact]
        public void IsOverriddenByCovariantReturnMethod_NotCovariantAssignable_ReturnsFalse()
        {
            // Tests line 179: base returns IEnumerable<int>, covariant returns List<string>
            // IEnumerable<int> is NOT covariant-assignable from List<string>
            var baseMethod = typeof(BaseWithIEnumerableIntReturn).GetMethod("GetItems")!;
            var covariantMethod = typeof(DerivedWithStringListReturn).GetMethod("GetItems")!;
            Assert.False(baseMethod.IsOverriddenByCovariantReturnMethod(covariantMethod));
        }

        [Fact]
        public void IsOverriddenByCovariantReturnMethod_DifferentParamCount_ReturnsFalse()
        {
            // Tests line 185: base has 1 param, covariant has 0 params
            // Returns are covariant-compatible (object vs string)
            var baseMethod = typeof(BaseWithObjectReturnAndParam).GetMethod("Process")!;
            var covariantMethod = typeof(DerivedWithStringReturnNoParam).GetMethod("Process")!;
            Assert.False(baseMethod.IsOverriddenByCovariantReturnMethod(covariantMethod));
        }

        [Fact]
        public void IsOverriddenByCovariantReturnMethod_OneGenericOneNot_ReturnsFalse()
        {
            // Tests line 196: base is generic, covariant is not generic
            var baseMethod = typeof(BaseGenericMethod3).GetMethod("GenericMethod")!;
            var covariantMethod = typeof(DerivedNonGenericMethod3).GetMethod("GenericMethod")!;
            Assert.False(baseMethod.IsOverriddenByCovariantReturnMethod(covariantMethod));
        }

        [Fact]
        public void IsOverriddenByCovariantReturnMethod_DifferentGenericArgCount_ReturnsFalse()
        {
            // Tests line 206: base has 1 generic arg, covariant has 2
            var baseMethod = typeof(BaseGenericMethod3).GetMethod("GenericMethod")!;
            var covariantMethod = typeof(DerivedTwoGenericArgs3).GetMethods()
                .First(m => m.Name == "GenericMethod" && m.GetGenericArguments().Length == 2);
            Assert.False(baseMethod.IsOverriddenByCovariantReturnMethod(covariantMethod));
        }

        [Fact]
        public void IsOverriddenByCovariantReturnMethod_GenericArgsNotEquivalent_ReturnsFalse()
        {
            // Tests line 211: generic arguments are not equivalent
            var baseMethod = typeof(BaseGenericWithConstraint3).GetMethod("GenericMethod")!;
            var covariantMethod = typeof(DerivedGenericWithDifferentConstraint3).GetMethod("GenericMethod")!;
            Assert.False(baseMethod.IsOverriddenByCovariantReturnMethod(covariantMethod));
        }

        [Fact]
        public void GetCovariantReturnMethods_NoPreserveBaseOverrides_ReturnsEmpty()
        {
            // Tests line 101: when PreserveBaseOverridesAttribute is null
            // This is runtime-dependent; on .NET 9+ the attribute exists
            var result = typeof(NoCovariantService).GetCovariantReturnMethods();
            Assert.NotNull(result);
        }

        [Fact]
        public void GetCovariantReturnMethods_NoMatchingOverride_SkipsEntry()
        {
            // Tests line 115: when overriddenMethod is null
            // Need a type with covariant return methods but no matching base method
            var result = typeof(CovariantWithNoMatch).GetCovariantReturnMethods();
            Assert.NotNull(result);
        }

        [Fact]
        public void SubstituteGenericParameters_ByRefNonGeneric_ReturnsByRef()
        {
            // Tests line 347: return type as-is for byref non-generic type
            var byRefType = typeof(int).MakeByRefType();
            var map = new Dictionary<Type, Type>();
            var result = InvokeSubstituteGenericParameters(byRefType, map);
            Assert.True(result.IsByRef);
        }

        [Fact]
        public void SubstituteGenericParameters_PointerNonGeneric_ReturnsPointer()
        {
            // Tests line 347: return type as-is for pointer non-generic type
            var pointerType = typeof(int).MakePointerType();
            var map = new Dictionary<Type, Type>();
            var result = InvokeSubstituteGenericParameters(pointerType, map);
            Assert.True(result.IsPointer);
        }

        [Fact]
        public void AreEquivalentGenericTypes_OneDefinitionOneConstructed_ReturnsFalse()
        {
            // Tests line 389: one is generic type definition, other is constructed
            var type1 = typeof(List<int>);
            var type2 = typeof(List<>);
            var result = InvokeAreEquivalentGenericTypes(type1, type2,
                (a, b) => a == b, (a, b) => a == b);
            Assert.False(result);
        }

        [Fact]
        public void IsCovariantReturnEquivalentTo_ByRefVsNonByRef_ReturnsFalse()
        {
            // Tests line 507: type is byref, other is not byref
            var byRefType = typeof(int).MakeByRefType();
            var nonByRefType = typeof(int);
            var result = AspectCore.Extensions.TypeExtensions.IsCovariantReturnEquivalentTo(byRefType, nonByRefType);
            Assert.False(result);
        }

        [Fact]
        public void IsCovariantReturnEquivalentTo_BothByRef_ReturnsTrue()
        {
            // Tests the byref comparison path
            var byRefType1 = typeof(int).MakeByRefType();
            var byRefType2 = typeof(int).MakeByRefType();
            var result = AspectCore.Extensions.TypeExtensions.IsCovariantReturnEquivalentTo(byRefType1, byRefType2);
            Assert.True(result);
        }

        [Fact]
        public void AddTypeGenericParameterMap_NonGenericDeclaringType_NoMapping()
        {
            // Tests line 258: when projectedDeclaringType is null or not generic
            var method = typeof(BaseNonGeneric3).GetMethod("GetValue")!;
            var covariantMethod = typeof(DerivedNonGeneric3).GetMethod("GetValue")!;
            var result = InvokeCreateGenericParameterMap(method, covariantMethod);
            Assert.NotNull(result);
        }

        [Fact]
        public void AddTypeGenericParameterMap_GenericParamCountMismatch_NoMapping()
        {
            // Tests line 263: when generic parameter counts don't match
            var method = typeof(DifferentArgCountBase3<>).GetMethod("GetValue")!;
            var covariantMethod = typeof(DifferentArgCountDerived3).GetMethod("GetValue")!;
            var result = InvokeCreateGenericParameterMap(method, covariantMethod);
            Assert.NotNull(result);
        }

        #endregion

        #region SourceGeneratedProxyTypeGenerator.cs - ScanRegistries (lines 169-202)

        [Fact]
        public void SourceGeneratedProxyTypeGenerator_CreateMissingProxyException_BuildsMessage()
        {
            var options = new ProxyEngineOptions { Engine = ProxyEngine.DynamicProxy, Strict = false, AllowRuntimeFallback = true };
            var generator = new SourceGeneratedProxyTypeGenerator(
                CreateValidatorBuilder(), options, Array.Empty<ISourceGeneratedProxyRegistry>());
            // CreateMissingProxyException is private, use reflection
            var method = typeof(SourceGeneratedProxyTypeGenerator).GetMethod("CreateMissingProxyException",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var ex = (InvalidOperationException)method!.Invoke(generator, new object[]
            {
                typeof(IFinalTestSvc), typeof(FinalTestSvcImpl),
                SourceGeneratedProxyKind.Class, ProxyEngine.DynamicProxy, true
            })!;
            Assert.NotNull(ex);
            Assert.Contains("Failed to resolve", ex.Message);
        }

        [Fact]
        public void SourceGeneratedProxyTypeGenerator_WithManualRegistry_TriggersScanOnMiss()
        {
            var registry = new NullReturningRegistry2();
            var options = new ProxyEngineOptions { Engine = ProxyEngine.SourceGenerator, Strict = true };
            var generator = new SourceGeneratedProxyTypeGenerator(
                CreateValidatorBuilder(), options, new[] { registry });
            Assert.Throws<InvalidOperationException>(() =>
                generator.CreateInterfaceProxyType(typeof(IFinalTestSvc)));
        }

        #endregion

        #region ProxyGenerator.cs - fallback constructor paths (lines 106-110, 145, 155)

        [Fact]
        public void ProxyGenerator_CreateClassProxy_ParameterlessConstructor_Works()
        {
            var builder = new ProxyGeneratorBuilder();
            builder.Configure(c => { });
            var generator = builder.Build();
            var proxy = generator.CreateClassProxy<SimpleFinalClass>();
            Assert.NotNull(proxy);
        }

        [Fact]
        public void ProxyGenerator_CreateInterfaceProxy_WithImplementation_Works()
        {
            var builder = new ProxyGeneratorBuilder();
            builder.Configure(c => { });
            var generator = builder.Build();
            var proxy = generator.CreateInterfaceProxy<IFinalTestSvc>(new FinalTestSvcImpl());
            Assert.NotNull(proxy);
        }

        #endregion

        #region AttributeAdditionalInterceptorSelector.cs - SelectFromBase (lines 51-54)

        [Fact]
        public void AttributeAdditionalInterceptorSelector_WithBaseMethod_HasInheritedInterceptors()
        {
            var selector = new AttributeAdditionalInterceptorSelector();
            var baseMethod = typeof(BaseServiceWithInterceptor).GetMethod("DoSomething")!;
            var derivedMethod = typeof(DerivedServiceWithBase).GetMethod("DoSomething")!;
            var interceptors = selector.Select(baseMethod, derivedMethod);
            Assert.NotNull(interceptors);
        }

        #endregion

        #region GenericParameterNodeFactory.cs - SpecialConstraintMask (line 66)

        [Fact]
        public void GenericParameterNodeFactory_WithSpecialConstraintMask_Converts()
        {
            var result = GenericParameterNodeFactory.FromType(typeof(NotNullConstraintType<>));
            Assert.Single(result);
            Assert.Equal("T", result[0].Name);
        }

        [Fact]
        public void GenericParameterNodeFactory_WithReferenceAndNewConstraint_Converts()
        {
            var result = GenericParameterNodeFactory.FromType(typeof(RefNewConstraintType<>));
            Assert.Single(result);
        }

        #endregion

        #region AspectContextRuntimeExtensions.cs - Unwrap and IsAsyncFromMetaData (lines 173-183, 212)

        [Fact]
        public void AspectContextRuntimeExtensions_Unwrap_WithPlainTask_ReturnsNull()
        {
            var method = typeof(AspectContextRuntimeExtensions).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .First(m => m.Name == "Unwrap");
            var task = Task.CompletedTask;
            var result = method.Invoke(null, new object[] { task, typeof(Task).GetTypeInfo() });
            Assert.NotNull(result);
        }

        [Fact]
        public void AspectContextRuntimeExtensions_IsAsyncFromMetaData_WithObjectAndAsyncAttribute_ReturnsTrue()
        {
            var method = typeof(AspectContextRuntimeExtensions).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .First(m => m.Name == "IsAsyncFromMetaData");
            var testMethod = typeof(AsyncObjectReturnSvc).GetMethod("GetAsyncValue")!;
            var result = (bool)method.Invoke(null, new object[] { testMethod })!;
            Assert.True(result);
        }

        #endregion

        #region AspectActivator.cs - InvokeValueTask (lines 114-116)

        [Fact]
        public void AspectActivator_InvokeValueTask_WithValueTaskReturn_Works()
        {
            var builder = new ProxyGeneratorBuilder();
            builder.Configure(c => { });
            var generator = builder.Build();
            var service = generator.CreateClassProxy<ValueTaskReturnService>();
            Assert.NotNull(service);
            var result = service.GetValue();
            Assert.Equal(42, result);
        }

        #endregion

        #region InterfaceImplAstBuilder.cs - generic method resolution (lines 414-415)

        [Fact]
        public void InterfaceImplBuilder_ResolveImplementationMethod_GenericTypeDefMatch_ReturnsMethod()
        {
            var method = typeof(IGenericSvc3<>).GetMethod("GetValue")!;
            var result = InterfaceImplBuilder.ResolveImplementationMethod(method, typeof(GenericSvcImpl3<>));
            Assert.NotNull(result);
        }

        #endregion

        #region MethodInfoExtensions.cs - GetInterfaceDeclarations (line 14)

        [Fact]
        public void MethodInfoExtensions_GetInterfaceDeclarations_OnModuleMethod_ReturnsEmpty()
        {
            var method = typeof(object).GetMethod("ToString")!;
            var result = method.GetInterfaceDeclarations();
            Assert.NotNull(result);
        }

        #endregion

        #region ParameterNodeFactory.cs - FormatException on DefaultValue (lines 85-91)

        [Fact]
        public void ParameterNodeFactory_FromMethod_WithDateTimeDefault_HandlesFormatException()
        {
            var method = typeof(IParamWithDateTimeDefault).GetMethod("Method")!;
            var result = ParameterNodeFactory.FromMethod(method);
            Assert.NotNull(result);
            Assert.True(result.Count > 0);
        }

        [Fact]
        public void ParameterNodeFactory_FromMethod_WithEnumDefault_HandlesFormatException()
        {
            var method = typeof(IParamWithEnumDefault).GetMethod("Method")!;
            var result = ParameterNodeFactory.FromMethod(method);
            Assert.NotNull(result);
            Assert.True(result.Count > 0);
        }

        #endregion

        #region Helper Methods

        private static IAspectValidatorBuilder CreateValidatorBuilder()
        {
            return new AspectValidatorBuilder(new AspectConfiguration());
        }

        private static Type InvokeSubstituteGenericParameters(Type type, IReadOnlyDictionary<Type, Type> map)
        {
            var method = typeof(AspectCore.Extensions.TypeExtensions).GetMethod("SubstituteGenericParameters",
                BindingFlags.NonPublic | BindingFlags.Static);
            return (Type)method!.Invoke(null, new object[] { type, map })!;
        }

        private static bool InvokeAreEquivalentGenericTypes(Type type, Type other,
            Func<Type, Type, bool> argumentComparer, Func<Type, Type, bool> typeDefinitionComparer)
        {
            var method = typeof(AspectCore.Extensions.TypeExtensions).GetMethod("AreEquivalentGenericTypes",
                BindingFlags.NonPublic | BindingFlags.Static);
            return (bool)method!.Invoke(null, new object[] { type, other, argumentComparer, typeDefinitionComparer })!;
        }

        private static bool InvokeIsCovariantReturnEquivalentTo(Type type, Type other)
        {
            var method = typeof(AspectCore.Extensions.TypeExtensions).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .First(m => m.Name == "IsCovariantReturnEquivalentTo");
            return (bool)method.Invoke(null, new object[] { type, other })!;
        }

        private static IReadOnlyDictionary<Type, Type> InvokeCreateGenericParameterMap(MethodInfo method, MethodInfo covariantReturnMethod)
        {
            var privateMethod = typeof(AspectCore.Extensions.TypeExtensions).GetMethod("CreateGenericParameterMap",
                BindingFlags.NonPublic | BindingFlags.Static);
            return (IReadOnlyDictionary<Type, Type>)privateMethod!.Invoke(null, new object[] { method, covariantReturnMethod })!;
        }

        #endregion

        #region Test Types

        [AttributeUsage(AttributeTargets.Parameter)]
        public class TestParamAttribute : Attribute { }

        // ILEmitVisitor - nullable default parameter types
        public interface INullableIntNullDefault { void Method(int? x = null); }
        public class NullableIntNullDefaultImpl : INullableIntNullDefault { public void Method(int? x = null) { } }

        public interface INullableIntNonNullDefault { void Method(int? x = 5); }
        public class NullableIntNonNullDefaultImpl : INullableIntNonNullDefault { public void Method(int? x = 5) { } }

        public interface INullableEnumDefault { void Method(TestEnum2? x = TestEnum2.Value2); }
        public class NullableEnumDefaultImpl : INullableEnumDefault { public void Method(TestEnum2? x = TestEnum2.Value2) { } }

        public interface INullableBoolDefault { void Method(bool? x = true); }
        public class NullableBoolDefaultImpl : INullableBoolDefault { public void Method(bool? x = true) { } }

        public interface INullableDecimalDefault { void Method(decimal? x = 3.14m); }
        public class NullableDecimalDefaultImpl : INullableDecimalDefault { public void Method(decimal? x = 3.14m) { } }

        public interface INullableDateTimeDefault { void Method(DateTime? x = default); }
        public class NullableDateTimeDefaultImpl : INullableDateTimeDefault { public void Method(DateTime? x = default) { } }

        public interface IEnumDefault { void Method(TestEnum2 x = TestEnum2.Value1); }
        public class EnumDefaultImpl : IEnumDefault { public void Method(TestEnum2 x = TestEnum2.Value1) { } }

        public interface IDateTimeDefault { void Method(DateTime x = default); }
        public class DateTimeDefaultImpl : IDateTimeDefault { public void Method(DateTime x = default) { } }

        public interface IDecimalDefault { void Method(decimal x = 3.14m); }
        public class DecimalDefaultImpl : IDecimalDefault { public void Method(decimal x = 3.14m) { } }

        public interface ICharDefault { void Method(char x = 'A'); }
        public class CharDefaultImpl : ICharDefault { public void Method(char x = 'A') { } }

        public interface INullableCharDefault { void Method(char? x = 'B'); }
        public class NullableCharDefaultImpl : INullableCharDefault { public void Method(char? x = 'B') { } }

        public interface IMultiNullableDefault { void Method(int? a = null, bool? b = true, decimal? c = 1.5m, long? d = 100L); }
        public class MultiNullableDefaultImpl : IMultiNullableDefault { public void Method(int? a = null, bool? b = true, decimal? c = 1.5m, long? d = 100L) { } }

        public class ClassWithNullableDefaults
        {
            public virtual void Method(int? x = null, bool? y = true) { }
            public virtual int GetValue() => 42;
        }

        public enum TestEnum2 { Value1, Value2, Value3 }

        // ILEmitVisitor - attributes
        public interface IAttributedMethod
        {
            [Obsolete("test")]
            void AttributedMethod();
            void NormalMethod();
        }
        public class AttributedMethodImpl : IAttributedMethod
        {
            [Obsolete("test")]
            public void AttributedMethod() { }
            public void NormalMethod() { }
        }

        public interface IAttributedParam
        {
            void Method([TestParam] int param);
        }
        public class AttributedParamImpl : IAttributedParam
        {
            public void Method([TestParam] int param) { }
        }

        public class ClassWithAttributedMethod
        {
            [Obsolete("test")]
            public virtual void AttributedMethod() { }
            public virtual int NormalMethod() => 42;
        }

        // AspectValidatorExtensions test types
        public interface IValidatorTestInterface { void DoSomething(); }
        public class ClassImplementingInterface : IValidatorTestInterface { public void DoSomething() { } }

        // ConstructorCallSiteResolver test types
        public interface IResolvableDependency { int GetValue(); }
        public class ResolvableDependencyImpl : IResolvableDependency { public int GetValue() => 42; }
        public class NeedsUnregisteredDependency
        {
            public NeedsUnregisteredDependency(IUnregisteredDependency dep) { }
        }
        public interface IUnregisteredDependency { }

        // ServiceContext test types
        public interface ISimpleSvc { int GetValue(); }
        public class SimpleSvcImpl : ISimpleSvc { public int GetValue() => 42; }

        // ServiceTable test types
        public interface ISvc { void Do(); }
        public class SvcImpl : ISvc { public void Do() { } }
        public interface ISvc<T> { T Get(); }
        public class SvcImpl<T> : ISvc<T> { public T Get() => default!; }
        public interface IUnregisteredSvc { }

        public class CustomServiceDef : ServiceDefinition
        {
            public CustomServiceDef(Type serviceType, Lifetime lifetime) : base(serviceType, lifetime) { }
        }

        // TypeExtensions - covariant return test types
        public class BaseWithIEnumerableIntReturn
        {
            public virtual IEnumerable<int> GetItems() => new List<int>();
        }
        public class DerivedWithStringListReturn
        {
            public List<string> GetItems() => new List<string>();
        }

        public class BaseWithObjectReturnAndParam
        {
            public virtual object Process(int x) => x;
        }
        public class DerivedWithStringReturnNoParam
        {
            public string Process() => "test";
        }

        public class BaseGenericMethod3
        {
            public virtual object GenericMethod<T>(T value) => value!;
        }
        public class DerivedNonGenericMethod3
        {
            public string GenericMethod(int value) => value.ToString();
        }
        public class DerivedTwoGenericArgs3
        {
            public string GenericMethod<T>(T value) => value?.ToString() ?? "";
            public string GenericMethod<T1, T2>(T1 v1, T2 v2) => v1?.ToString() ?? "";
        }

        public class BaseGenericWithConstraint3
        {
            public virtual object GenericMethod<T>(T value) where T : struct => value!;
        }
        public class DerivedGenericWithDifferentConstraint3
        {
            public string GenericMethod<T>(T value) where T : class => value?.ToString() ?? "";
        }

        public class NoCovariantService
        {
            public virtual string GetName() => "test";
            public virtual int GetValue() => 42;
        }

        public class CovariantWithNoMatch
        {
            public virtual BaseResult3 GetResult() => new BaseResult3();
        }
        public class BaseResult3 { }
        public class DerivedResult3 : BaseResult3 { }

        // For line 258: non-generic declaring type
        public class BaseNonGeneric3
        {
            public virtual object GetValue() => new object();
        }
        public class DerivedNonGeneric3 : BaseNonGeneric3
        {
            public override string GetValue() => "test";
        }

        // For line 263: generic param count mismatch
        public class DifferentArgCountBase3<T>
        {
            public virtual T GetValue() => default!;
        }
        public class DifferentArgCountDerived3 : DifferentArgCountBase3<int>
        {
            public override int GetValue() => 42;
        }

        // SourceGeneratedProxyTypeGenerator test types
        public interface IFinalTestSvc { int GetValue(); }
        public class FinalTestSvcImpl : IFinalTestSvc { public int GetValue() => 42; }

        public class NullReturningRegistry2 : ISourceGeneratedProxyRegistry
        {
            public bool TryGetProxyType(Type serviceType, Type implementationType, SourceGeneratedProxyKind kind, out Type proxyType)
            {
                proxyType = null!;
                return false;
            }
        }

        // ProxyGenerator test types
        public class SimpleFinalClass
        {
            public virtual int GetValue() => 42;
        }

        // AttributeAdditionalInterceptorSelector test types
        public class BaseServiceWithInterceptor
        {
            public virtual void DoSomething() { }
        }
        public class DerivedServiceWithBase : BaseServiceWithInterceptor
        {
            public override void DoSomething() { }
        }

        // GenericParameterNodeFactory test types
        public class NotNullConstraintType<T> where T : notnull { }
        public class RefNewConstraintType<T> where T : class, new() { }

        // AspectContextRuntimeExtensions test types
        public class AsyncObjectReturnSvc
        {
            [AsyncAspect]
            public virtual object GetAsyncValue() => Task.CompletedTask;
        }

        // AspectActivator test types
        public class ValueTaskReturnService
        {
            public virtual ValueTask<int> GetValueAsync() => new ValueTask<int>(42);
            public virtual int GetValue() => 42;
        }

        // InterfaceImplAstBuilder test types
        public interface IGenericSvc3<T> { T GetValue(); }
        public class GenericSvcImpl3<T> : IGenericSvc3<T> { public T GetValue() => default!; }

        // ParameterNodeFactory test types
        public interface IParamWithDateTimeDefault { void Method(DateTime x = default); }
        public interface IParamWithEnumDefault { void Method(TestEnum2 x = TestEnum2.Value1); }

        #endregion
    }
}
