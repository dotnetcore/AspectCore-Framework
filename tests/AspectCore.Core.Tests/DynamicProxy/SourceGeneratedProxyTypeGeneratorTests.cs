using System;
using System.Collections.Generic;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class SourceGeneratedProxyTypeGeneratorTests
    {
        private static IAspectValidatorBuilder CreateValidatorBuilder()
        {
            return new AspectValidatorBuilder(new AspectConfiguration());
        }

        private static SourceGeneratedProxyTypeGenerator CreateGenerator(
            ProxyEngineOptions options,
            IEnumerable<ISourceGeneratedProxyRegistry> registries = null)
        {
            return new SourceGeneratedProxyTypeGenerator(CreateValidatorBuilder(), options, registries);
        }

        private static ProxyEngineOptions CreateOptions(
            ProxyEngine engine,
            bool strict = false,
            bool? allowRuntimeFallback = null)
        {
            return new ProxyEngineOptions
            {
                Engine = engine,
                Strict = strict,
                AllowRuntimeFallback = allowRuntimeFallback,
            };
        }

        #region Constructor

        [Fact]
        public void Constructor_NullAspectValidatorBuilder_ThrowsArgumentNullException()
        {
            var options = CreateOptions(ProxyEngine.Auto);
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new SourceGeneratedProxyTypeGenerator(null, options, Array.Empty<ISourceGeneratedProxyRegistry>()));
            Assert.Equal("aspectValidatorBuilder", ex.ParamName);
        }

        [Fact]
        public void Constructor_NullOptions_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new SourceGeneratedProxyTypeGenerator(CreateValidatorBuilder(), null, Array.Empty<ISourceGeneratedProxyRegistry>()));
            Assert.Equal("options", ex.ParamName);
        }

        [Fact]
        public void Constructor_NullRegistries_TreatedAsEmpty()
        {
            var options = CreateOptions(ProxyEngine.DynamicProxy);
            var generator = CreateGenerator(options, null);
            Assert.NotNull(generator);
            // Should work without throwing — falls back to dynamic proxy
            var proxyType = generator.CreateInterfaceProxyType(typeof(ISgTestService));
            Assert.NotNull(proxyType);
        }

        [Fact]
        public void Constructor_FiltersOutNullRegistryEntries()
        {
            var options = CreateOptions(ProxyEngine.DynamicProxy);
            var registries = new ISourceGeneratedProxyRegistry[] { null, null };
            var generator = CreateGenerator(options, registries);
            Assert.NotNull(generator);
            // Should work without throwing — null entries filtered out
            var proxyType = generator.CreateClassProxyType(typeof(SgTestClass), typeof(SgTestClass));
            Assert.NotNull(proxyType);
        }

        [Fact]
        public void Constructor_ValidArguments_CreatesInstance()
        {
            var options = CreateOptions(ProxyEngine.Auto);
            var generator = CreateGenerator(options);
            Assert.NotNull(generator);
        }

        #endregion

        #region CreateInterfaceProxyType(Type) - DynamicProxy Engine

        [Fact]
        public void CreateInterfaceProxyType_DynamicProxyEngine_FallsBackToDynamicProxy()
        {
            var options = CreateOptions(ProxyEngine.DynamicProxy);
            var generator = CreateGenerator(options);
            var proxyType = generator.CreateInterfaceProxyType(typeof(ISgTestService));
            Assert.NotNull(proxyType);
            Assert.True(typeof(ISgTestService).IsAssignableFrom(proxyType));
            Assert.NotEqual(typeof(ISgTestService), proxyType);
        }

        [Fact]
        public void CreateInterfaceProxyType_DynamicProxyEngine_NullServiceType_ThrowsArgumentNullException()
        {
            var options = CreateOptions(ProxyEngine.DynamicProxy);
            var generator = CreateGenerator(options);
            var ex = Assert.Throws<ArgumentNullException>(() => generator.CreateInterfaceProxyType(null));
            Assert.Equal("serviceType", ex.ParamName);
        }

        #endregion

        #region CreateInterfaceProxyType(Type) - SourceGenerator Engine

        [Fact]
        public void CreateInterfaceProxyType_SourceGeneratorEngine_EmptyRegistry_ThrowsInvalidOperationException()
        {
            var options = CreateOptions(ProxyEngine.SourceGenerator);
            var generator = CreateGenerator(options);
            var ex = Assert.Throws<InvalidOperationException>(() =>
                generator.CreateInterfaceProxyType(typeof(ISgTestService)));
            Assert.Contains("Failed to resolve source-generated proxy type.", ex.Message, StringComparison.Ordinal);
            Assert.Contains("Engine: SourceGenerator", ex.Message, StringComparison.Ordinal);
            Assert.Contains(typeof(ISgTestService).FullName, ex.Message, StringComparison.Ordinal);
            Assert.Contains("Hint:", ex.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void CreateInterfaceProxyType_SourceGeneratorEngine_WithMatchingRegistry_ReturnsResolvedType()
        {
            var registry = new MockRegistry((serviceType, implType, kind) =>
                serviceType == typeof(ISgTestService) ? typeof(SgTestServiceProxy) : null);
            var options = CreateOptions(ProxyEngine.SourceGenerator);
            var generator = CreateGenerator(options, new[] { registry });
            var proxyType = generator.CreateInterfaceProxyType(typeof(ISgTestService));
            Assert.Equal(typeof(SgTestServiceProxy), proxyType);
        }

        [Fact]
        public void CreateInterfaceProxyType_SourceGeneratorEngine_WithMatchingRegistry_CachesResult()
        {
            var callCount = 0;
            var registry = new MockRegistry((serviceType, implType, kind) =>
            {
                if (serviceType == typeof(ISgTestService))
                {
                    callCount++;
                    return typeof(SgTestServiceProxy);
                }
                return null;
            });
            var options = CreateOptions(ProxyEngine.SourceGenerator);
            var generator = CreateGenerator(options, new[] { registry });

            var first = generator.CreateInterfaceProxyType(typeof(ISgTestService));
            var second = generator.CreateInterfaceProxyType(typeof(ISgTestService));

            Assert.Same(first, second);
            Assert.Equal(1, callCount);
        }

        [Fact]
        public void CreateInterfaceProxyType_SourceGeneratorEngine_NullServiceType_ThrowsArgumentNullException()
        {
            var options = CreateOptions(ProxyEngine.SourceGenerator);
            var generator = CreateGenerator(options);
            var ex = Assert.Throws<ArgumentNullException>(() => generator.CreateInterfaceProxyType(null));
            Assert.Equal("serviceType", ex.ParamName);
        }

        #endregion

        #region CreateInterfaceProxyType(Type) - Auto Engine

        [Fact]
        public void CreateInterfaceProxyType_AutoEngine_EmptyRegistry_FallsBackToDynamicProxy()
        {
            var options = CreateOptions(ProxyEngine.Auto);
            var generator = CreateGenerator(options);
            var proxyType = generator.CreateInterfaceProxyType(typeof(ISgTestService));
            Assert.NotNull(proxyType);
            Assert.True(typeof(ISgTestService).IsAssignableFrom(proxyType));
            Assert.NotEqual(typeof(ISgTestService), proxyType);
        }

        [Fact]
        public void CreateInterfaceProxyType_AutoEngine_WithMatchingRegistry_ReturnsResolvedType()
        {
            var registry = new MockRegistry((serviceType, implType, kind) =>
                serviceType == typeof(ISgTestService) ? typeof(SgTestServiceProxy) : null);
            var options = CreateOptions(ProxyEngine.Auto);
            var generator = CreateGenerator(options, new[] { registry });
            var proxyType = generator.CreateInterfaceProxyType(typeof(ISgTestService));
            Assert.Equal(typeof(SgTestServiceProxy), proxyType);
        }

        [Fact]
        public void CreateInterfaceProxyType_AutoEngine_Strict_EmptyRegistry_ThrowsInvalidOperationException()
        {
            var options = CreateOptions(ProxyEngine.Auto, strict: true);
            var generator = CreateGenerator(options);
            var ex = Assert.Throws<InvalidOperationException>(() =>
                generator.CreateInterfaceProxyType(typeof(ISgTestService)));
            Assert.Contains("Failed to resolve source-generated proxy type.", ex.Message, StringComparison.Ordinal);
            Assert.Contains("Strict: True", ex.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void CreateInterfaceProxyType_AutoEngine_AllowRuntimeFallbackFalse_EmptyRegistry_Throws()
        {
            var options = CreateOptions(ProxyEngine.Auto, allowRuntimeFallback: false);
            var generator = CreateGenerator(options);
            var ex = Assert.Throws<InvalidOperationException>(() =>
                generator.CreateInterfaceProxyType(typeof(ISgTestService)));
            Assert.Contains("Failed to resolve source-generated proxy type.", ex.Message, StringComparison.Ordinal);
            Assert.Contains("AllowRuntimeFallback: False", ex.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void CreateInterfaceProxyType_AutoEngine_AllowRuntimeFallbackTrue_EmptyRegistry_FallsBack()
        {
            var options = CreateOptions(ProxyEngine.Auto, allowRuntimeFallback: true);
            var generator = CreateGenerator(options);
            var proxyType = generator.CreateInterfaceProxyType(typeof(ISgTestService));
            Assert.NotNull(proxyType);
            Assert.True(typeof(ISgTestService).IsAssignableFrom(proxyType));
            Assert.NotEqual(typeof(ISgTestService), proxyType);
        }

        #endregion

        #region CreateInterfaceProxyType(Type, Type) - with implementation type

        [Fact]
        public void CreateInterfaceProxyType_WithImpl_NullServiceType_ThrowsArgumentNullException()
        {
            var options = CreateOptions(ProxyEngine.Auto);
            var generator = CreateGenerator(options);
            var ex = Assert.Throws<ArgumentNullException>(() =>
                generator.CreateInterfaceProxyType(null, typeof(SgTestService)));
            Assert.Equal("serviceType", ex.ParamName);
        }

        [Fact]
        public void CreateInterfaceProxyType_WithImpl_DynamicProxyEngine_FallsBackToDynamicProxy()
        {
            var options = CreateOptions(ProxyEngine.DynamicProxy);
            var generator = CreateGenerator(options);
            var proxyType = generator.CreateInterfaceProxyType(typeof(ISgTestService), typeof(SgTestService));
            Assert.NotNull(proxyType);
            Assert.True(typeof(ISgTestService).IsAssignableFrom(proxyType));
            Assert.NotEqual(typeof(ISgTestService), proxyType);
        }

        [Fact]
        public void CreateInterfaceProxyType_WithImpl_SourceGeneratorEngine_EmptyRegistry_Throws()
        {
            var options = CreateOptions(ProxyEngine.SourceGenerator);
            var generator = CreateGenerator(options);
            Assert.Throws<InvalidOperationException>(() =>
                generator.CreateInterfaceProxyType(typeof(ISgTestService), typeof(SgTestService)));
        }

        [Fact]
        public void CreateInterfaceProxyType_WithImpl_SourceGeneratorEngine_WithMatchingRegistry_ReturnsResolvedType()
        {
            // Interface proxy lookup uses implementationType=null for registry lookup
            var registry = new MockRegistry((serviceType, implType, kind) =>
                serviceType == typeof(ISgTestService) && implType == null ? typeof(SgTestServiceProxy) : null);
            var options = CreateOptions(ProxyEngine.SourceGenerator);
            var generator = CreateGenerator(options, new[] { registry });
            var proxyType = generator.CreateInterfaceProxyType(typeof(ISgTestService), typeof(SgTestService));
            Assert.Equal(typeof(SgTestServiceProxy), proxyType);
        }

        [Fact]
        public void CreateInterfaceProxyType_WithImpl_AutoEngine_EmptyRegistry_FallsBackToDynamicProxy()
        {
            var options = CreateOptions(ProxyEngine.Auto);
            var generator = CreateGenerator(options);
            var proxyType = generator.CreateInterfaceProxyType(typeof(ISgTestService), typeof(SgTestService));
            Assert.NotNull(proxyType);
            Assert.True(typeof(ISgTestService).IsAssignableFrom(proxyType));
            Assert.NotEqual(typeof(ISgTestService), proxyType);
        }

        #endregion

        #region CreateClassProxyType - DynamicProxy Engine

        [Fact]
        public void CreateClassProxyType_DynamicProxyEngine_FallsBackToDynamicProxy()
        {
            var options = CreateOptions(ProxyEngine.DynamicProxy);
            var generator = CreateGenerator(options);
            var proxyType = generator.CreateClassProxyType(typeof(SgTestClass), typeof(SgTestClass));
            Assert.NotNull(proxyType);
            Assert.True(typeof(SgTestClass).IsAssignableFrom(proxyType));
            Assert.NotEqual(typeof(SgTestClass), proxyType);
        }

        [Fact]
        public void CreateClassProxyType_DynamicProxyEngine_NullServiceType_ThrowsArgumentNullException()
        {
            var options = CreateOptions(ProxyEngine.DynamicProxy);
            var generator = CreateGenerator(options);
            var ex = Assert.Throws<ArgumentNullException>(() =>
                generator.CreateClassProxyType(null, typeof(SgTestClass)));
            Assert.Equal("serviceType", ex.ParamName);
        }

        [Fact]
        public void CreateClassProxyType_DynamicProxyEngine_NullImplementationType_ThrowsArgumentNullException()
        {
            var options = CreateOptions(ProxyEngine.DynamicProxy);
            var generator = CreateGenerator(options);
            var ex = Assert.Throws<ArgumentNullException>(() =>
                generator.CreateClassProxyType(typeof(SgTestClass), null));
            Assert.Equal("implementationType", ex.ParamName);
        }

        #endregion

        #region CreateClassProxyType - SourceGenerator Engine

        [Fact]
        public void CreateClassProxyType_SourceGeneratorEngine_EmptyRegistry_ThrowsInvalidOperationException()
        {
            var options = CreateOptions(ProxyEngine.SourceGenerator);
            var generator = CreateGenerator(options);
            var ex = Assert.Throws<InvalidOperationException>(() =>
                generator.CreateClassProxyType(typeof(SgTestClass), typeof(SgTestClass)));
            Assert.Contains("Failed to resolve source-generated proxy type.", ex.Message, StringComparison.Ordinal);
            Assert.Contains("Kind: Class", ex.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void CreateClassProxyType_SourceGeneratorEngine_WithMatchingRegistry_ReturnsResolvedType()
        {
            var registry = new MockRegistry((serviceType, implType, kind) =>
                serviceType == typeof(SgTestClass) && kind == SourceGeneratedProxyKind.Class
                    ? typeof(SgTestClassProxy) : null);
            var options = CreateOptions(ProxyEngine.SourceGenerator);
            var generator = CreateGenerator(options, new[] { registry });
            var proxyType = generator.CreateClassProxyType(typeof(SgTestClass), typeof(SgTestClass));
            Assert.Equal(typeof(SgTestClassProxy), proxyType);
        }

        [Fact]
        public void CreateClassProxyType_SourceGeneratorEngine_WithMatchingRegistry_CachesResult()
        {
            var callCount = 0;
            var registry = new MockRegistry((serviceType, implType, kind) =>
            {
                if (serviceType == typeof(SgTestClass) && kind == SourceGeneratedProxyKind.Class)
                {
                    callCount++;
                    return typeof(SgTestClassProxy);
                }
                return null;
            });
            var options = CreateOptions(ProxyEngine.SourceGenerator);
            var generator = CreateGenerator(options, new[] { registry });

            var first = generator.CreateClassProxyType(typeof(SgTestClass), typeof(SgTestClass));
            var second = generator.CreateClassProxyType(typeof(SgTestClass), typeof(SgTestClass));

            Assert.Same(first, second);
            Assert.Equal(1, callCount);
        }

        [Fact]
        public void CreateClassProxyType_SourceGeneratorEngine_NullServiceType_ThrowsArgumentNullException()
        {
            var options = CreateOptions(ProxyEngine.SourceGenerator);
            var generator = CreateGenerator(options);
            var ex = Assert.Throws<ArgumentNullException>(() =>
                generator.CreateClassProxyType(null, typeof(SgTestClass)));
            Assert.Equal("serviceType", ex.ParamName);
        }

        [Fact]
        public void CreateClassProxyType_SourceGeneratorEngine_NullImplementationType_ThrowsArgumentNullException()
        {
            var options = CreateOptions(ProxyEngine.SourceGenerator);
            var generator = CreateGenerator(options);
            var ex = Assert.Throws<ArgumentNullException>(() =>
                generator.CreateClassProxyType(typeof(SgTestClass), null));
            Assert.Equal("implementationType", ex.ParamName);
        }

        #endregion

        #region CreateClassProxyType - Auto Engine

        [Fact]
        public void CreateClassProxyType_AutoEngine_EmptyRegistry_FallsBackToDynamicProxy()
        {
            var options = CreateOptions(ProxyEngine.Auto);
            var generator = CreateGenerator(options);
            var proxyType = generator.CreateClassProxyType(typeof(SgTestClass), typeof(SgTestClass));
            Assert.NotNull(proxyType);
            Assert.True(typeof(SgTestClass).IsAssignableFrom(proxyType));
            Assert.NotEqual(typeof(SgTestClass), proxyType);
        }

        [Fact]
        public void CreateClassProxyType_AutoEngine_WithMatchingRegistry_ReturnsResolvedType()
        {
            var registry = new MockRegistry((serviceType, implType, kind) =>
                serviceType == typeof(SgTestClass) && kind == SourceGeneratedProxyKind.Class
                    ? typeof(SgTestClassProxy) : null);
            var options = CreateOptions(ProxyEngine.Auto);
            var generator = CreateGenerator(options, new[] { registry });
            var proxyType = generator.CreateClassProxyType(typeof(SgTestClass), typeof(SgTestClass));
            Assert.Equal(typeof(SgTestClassProxy), proxyType);
        }

        [Fact]
        public void CreateClassProxyType_AutoEngine_Strict_EmptyRegistry_Throws()
        {
            var options = CreateOptions(ProxyEngine.Auto, strict: true);
            var generator = CreateGenerator(options);
            Assert.Throws<InvalidOperationException>(() =>
                generator.CreateClassProxyType(typeof(SgTestClass), typeof(SgTestClass)));
        }

        [Fact]
        public void CreateClassProxyType_AutoEngine_AllowRuntimeFallbackFalse_EmptyRegistry_Throws()
        {
            var options = CreateOptions(ProxyEngine.Auto, allowRuntimeFallback: false);
            var generator = CreateGenerator(options);
            Assert.Throws<InvalidOperationException>(() =>
                generator.CreateClassProxyType(typeof(SgTestClass), typeof(SgTestClass)));
        }

        [Fact]
        public void CreateClassProxyType_AutoEngine_AllowRuntimeFallbackTrue_EmptyRegistry_FallsBack()
        {
            var options = CreateOptions(ProxyEngine.Auto, allowRuntimeFallback: true);
            var generator = CreateGenerator(options);
            var proxyType = generator.CreateClassProxyType(typeof(SgTestClass), typeof(SgTestClass));
            Assert.NotNull(proxyType);
            Assert.True(typeof(SgTestClass).IsAssignableFrom(proxyType));
            Assert.NotEqual(typeof(SgTestClass), proxyType);
        }

        #endregion

        #region SourceGenerator Engine - AllowRuntimeFallback (should not affect since SG always fails on miss)

        [Fact]
        public void CreateInterfaceProxyType_SourceGeneratorEngine_AllowRuntimeFallbackTrue_StillThrowsOnMiss()
        {
            // SourceGenerator engine: mustFail is always true (engine == SourceGenerator),
            // so AllowRuntimeFallback does not help — it always throws on miss.
            var options = CreateOptions(ProxyEngine.SourceGenerator, allowRuntimeFallback: true);
            var generator = CreateGenerator(options);
            Assert.Throws<InvalidOperationException>(() =>
                generator.CreateInterfaceProxyType(typeof(ISgTestService)));
        }

        #endregion

        #region Registry Priority (manual vs scanned)

        [Fact]
        public void CreateInterfaceProxyType_ManualRegistry_TakesPriorityOverScanned()
        {
            var manualRegistry = new MockRegistry((serviceType, implType, kind) =>
                serviceType == typeof(ISgTestService) ? typeof(SgTestServiceProxy) : null);
            var options = CreateOptions(ProxyEngine.SourceGenerator);
            var generator = CreateGenerator(options, new[] { manualRegistry });
            var proxyType = generator.CreateInterfaceProxyType(typeof(ISgTestService));
            Assert.Equal(typeof(SgTestServiceProxy), proxyType);
        }

        [Fact]
        public void CreateInterfaceProxyType_FirstMatchingRegistry_Wins()
        {
            var registry1 = new MockRegistry((serviceType, implType, kind) =>
                serviceType == typeof(ISgTestService) ? typeof(SgTestServiceProxy) : null);
            var registry2 = new MockRegistry((serviceType, implType, kind) =>
                serviceType == typeof(ISgTestService) ? typeof(AlternateServiceProxy) : null);
            var options = CreateOptions(ProxyEngine.SourceGenerator);
            var generator = CreateGenerator(options, new[] { registry1, registry2 });
            var proxyType = generator.CreateInterfaceProxyType(typeof(ISgTestService));
            Assert.Equal(typeof(SgTestServiceProxy), proxyType);
        }

        #endregion

        #region Generic Type Closing (MakeGenericType)

        [Fact]
        public void CreateInterfaceProxyType_ClosedGenericService_OpenGenericRegistry_ClosesType()
        {
            var registry = new MockRegistry((serviceType, implType, kind) =>
                serviceType == typeof(IGenericTestService<int>) ? typeof(GenericTestProxy<>) : null);
            var options = CreateOptions(ProxyEngine.SourceGenerator);
            var generator = CreateGenerator(options, new[] { registry });

            var proxyType = generator.CreateInterfaceProxyType(typeof(IGenericTestService<int>));

            Assert.NotNull(proxyType);
            Assert.Equal(typeof(GenericTestProxy<int>), proxyType);
        }

        [Fact]
        public void CreateInterfaceProxyType_ClosedGenericService_CachesClosedType()
        {
            var callCount = 0;
            var registry = new MockRegistry((serviceType, implType, kind) =>
            {
                if (serviceType == typeof(IGenericTestService<int>))
                {
                    callCount++;
                    return typeof(GenericTestProxy<>);
                }
                return null;
            });
            var options = CreateOptions(ProxyEngine.SourceGenerator);
            var generator = CreateGenerator(options, new[] { registry });

            var first = generator.CreateInterfaceProxyType(typeof(IGenericTestService<int>));
            var second = generator.CreateInterfaceProxyType(typeof(IGenericTestService<int>));

            Assert.Same(first, second);
            Assert.Equal(1, callCount);
        }

        [Fact]
        public void CreateInterfaceProxyType_DifferentGenericArguments_ReturnDifferentClosedTypes()
        {
            var registry = new MockRegistry((serviceType, implType, kind) =>
            {
                if (serviceType == typeof(IGenericTestService<int>) ||
                    serviceType == typeof(IGenericTestService<string>))
                {
                    return typeof(GenericTestProxy<>);
                }
                return null;
            });
            var options = CreateOptions(ProxyEngine.SourceGenerator);
            var generator = CreateGenerator(options, new[] { registry });

            var intProxy = generator.CreateInterfaceProxyType(typeof(IGenericTestService<int>));
            var stringProxy = generator.CreateInterfaceProxyType(typeof(IGenericTestService<string>));

            Assert.Equal(typeof(GenericTestProxy<int>), intProxy);
            Assert.Equal(typeof(GenericTestProxy<string>), stringProxy);
            Assert.NotSame(intProxy, stringProxy);
        }

        #endregion

        #region Exception Message Content

        [Fact]
        public void CreateInterfaceProxyType_SourceGenerator_Missing_ExceptionContainsDiagnostics()
        {
            var options = CreateOptions(ProxyEngine.SourceGenerator, strict: true, allowRuntimeFallback: false);
            var generator = CreateGenerator(options);
            var ex = Assert.Throws<InvalidOperationException>(() =>
                generator.CreateInterfaceProxyType(typeof(ISgTestService)));

            Assert.Contains("Engine: SourceGenerator", ex.Message, StringComparison.Ordinal);
            Assert.Contains("Strict: True", ex.Message, StringComparison.Ordinal);
            Assert.Contains("AllowRuntimeFallback: False", ex.Message, StringComparison.Ordinal);
            Assert.Contains("Kind: Interface", ex.Message, StringComparison.Ordinal);
            Assert.Contains("ServiceType: " + typeof(ISgTestService).FullName, ex.Message, StringComparison.Ordinal);
            Assert.Contains("ImplementationType: <null>", ex.Message, StringComparison.Ordinal);
            Assert.Contains("ManualRegistries: 0", ex.Message, StringComparison.Ordinal);
            Assert.Contains("Hint:", ex.Message, StringComparison.Ordinal);
        }

        #endregion

        #region Mock Registry

        private sealed class MockRegistry : ISourceGeneratedProxyRegistry
        {
            private readonly Func<Type, Type, SourceGeneratedProxyKind, Type> _resolver;

            public MockRegistry(Func<Type, Type, SourceGeneratedProxyKind, Type> resolver)
            {
                _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            }

            public bool TryGetProxyType(Type serviceType, Type implementationType, SourceGeneratedProxyKind kind, out Type proxyType)
            {
                proxyType = _resolver(serviceType, implementationType, kind);
                return proxyType != null;
            }
        }

        #endregion

        #region Test Types

        public interface ISgTestService
        {
            int Add(int a, int b);
            string Name { get; }
        }

        public class SgTestService : ISgTestService
        {
            public int Add(int a, int b) => a + b;
            public string Name => "SgTestService";
        }

        public class SgTestClass
        {
            public virtual int Add(int a, int b) => a + b;
            public virtual string Name => "SgTestClass";
        }

        // Proxy types returned by the mock registry — stand-ins for source-generated proxies
        public class SgTestServiceProxy : ISgTestService
        {
            public int Add(int a, int b) => a + b;
            public string Name => "SgTestServiceProxy";
        }

        public class AlternateServiceProxy : ISgTestService
        {
            public int Add(int a, int b) => a + b;
            public string Name => "AlternateServiceProxy";
        }

        public class SgTestClassProxy : SgTestClass
        {
            public override string Name => "SgTestClassProxy";
        }

        // Generic types for MakeGenericType closing tests
        public interface IGenericTestService<T>
        {
            T Echo(T value);
        }

        public class GenericTestProxy<T> : IGenericTestService<T>
        {
            public T Echo(T value) => value;
        }

        #endregion
    }
}
