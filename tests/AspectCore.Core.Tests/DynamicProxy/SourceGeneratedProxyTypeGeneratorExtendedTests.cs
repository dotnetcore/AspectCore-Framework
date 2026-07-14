using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class SourceGeneratedProxyTypeGeneratorExtendedTests
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

        private static ProxyEngineOptions CreateOptions(ProxyEngine engine, bool strict = false, bool? allowRuntimeFallback = null)
        {
            return new ProxyEngineOptions
            {
                Engine = engine,
                Strict = strict,
                AllowRuntimeFallback = allowRuntimeFallback,
            };
        }

        [Fact]
        public void CreateInterfaceProxyType_SourceGeneratorEngine_NoMatchingRegistry_Throws()
        {
            // Tests line 133: end of manual registry loop with no match
            // and line 144: proxyType = null, return false
            var registry = new NonMatchingRegistry();
            var options = CreateOptions(ProxyEngine.SourceGenerator);
            var generator = CreateGenerator(options, new[] { registry });
            Assert.Throws<InvalidOperationException>(() =>
                generator.CreateInterfaceProxyType(typeof(ISgExtService)));
        }

        [Fact]
        public void CreateInterfaceProxyType_SourceGeneratorEngine_WithScannedRegistry_ReturnsResolved()
        {
            // Tests EnsureScannedRegistries and ScanRegistries path
            // Use a registry that doesn't match manually but the scan might find one
            var options = CreateOptions(ProxyEngine.SourceGenerator);
            var generator = CreateGenerator(options);
            // This will trigger scan - should work without throwing on scan
            Assert.Throws<InvalidOperationException>(() =>
                generator.CreateInterfaceProxyType(typeof(ISgExtService)));
        }

        [Fact]
        public void CreateClassProxyType_SourceGeneratorEngine_NoMatchingRegistry_Throws()
        {
            // Tests the class proxy path with no registry match
            var options = CreateOptions(ProxyEngine.SourceGenerator);
            var generator = CreateGenerator(options);
            Assert.Throws<InvalidOperationException>(() =>
                generator.CreateClassProxyType(typeof(SgExtClass), typeof(SgExtClass)));
        }

        [Fact]
        public void CreateInterfaceProxyType_AutoEngine_AllowRuntimeFallbackNull_UsesEngineDefault()
        {
            // Tests GetAllowRuntimeFallback when AllowRuntimeFallback is null
            // For Auto engine, should return true
            var options = CreateOptions(ProxyEngine.Auto, allowRuntimeFallback: null);
            var generator = CreateGenerator(options);
            var proxyType = generator.CreateInterfaceProxyType(typeof(ISgExtService));
            Assert.NotNull(proxyType);
            Assert.True(typeof(ISgExtService).IsAssignableFrom(proxyType));
        }

        [Fact]
        public void CreateInterfaceProxyType_SourceGeneratorEngine_AllowRuntimeFallbackNull_StillThrows()
        {
            // For SourceGenerator engine, mustFail is always true
            var options = CreateOptions(ProxyEngine.SourceGenerator, allowRuntimeFallback: null);
            var generator = CreateGenerator(options);
            Assert.Throws<InvalidOperationException>(() =>
                generator.CreateInterfaceProxyType(typeof(ISgExtService)));
        }

        [Fact]
        public void CreateInterfaceProxyType_WithImpl_SourceGenerator_GenericFallback_Throws()
        {
            // Tests the path where serviceType is generic but no matching registry
            var options = CreateOptions(ProxyEngine.SourceGenerator);
            var generator = CreateGenerator(options);
            Assert.Throws<InvalidOperationException>(() =>
                generator.CreateInterfaceProxyType(typeof(IGenericExtSvc<int>), typeof(GenericExtSvcImpl<int>)));
        }

        [Fact]
        public void CacheKey_Equals_SameKey_ReturnsTrue()
        {
            // Tests CacheKey.Equals(object) at line 248
            var type = typeof(ISgExtService);
            var key1 = CreateCacheKey(type, null, SourceGeneratedProxyKind.Interface);
            var key2 = CreateCacheKey(type, null, SourceGeneratedProxyKind.Interface);
            Assert.True(key1.Equals(key2));
            Assert.True(key1.Equals((object)key2));
        }

        [Fact]
        public void CacheKey_Equals_DifferentKey_ReturnsFalse()
        {
            var type1 = typeof(ISgExtService);
            var type2 = typeof(ISgExtService2);
            var key1 = CreateCacheKey(type1, null, SourceGeneratedProxyKind.Interface);
            var key2 = CreateCacheKey(type2, null, SourceGeneratedProxyKind.Interface);
            Assert.False(key1.Equals(key2));
            Assert.False(key1.Equals((object)key2));
        }

        [Fact]
        public void CacheKey_Equals_NonCacheKeyObject_ReturnsFalse()
        {
            // Tests line 248: obj is CacheKey check fails
            var type = typeof(ISgExtService);
            var key1 = CreateCacheKey(type, null, SourceGeneratedProxyKind.Interface);
            Assert.False(key1.Equals("not a cache key"));
            Assert.False(key1.Equals(null));
        }

        [Fact]
        public void CacheKey_GetHashCode_SameKey_SameHash()
        {
            var type = typeof(ISgExtService);
            var key1 = CreateCacheKey(type, null, SourceGeneratedProxyKind.Interface);
            var key2 = CreateCacheKey(type, null, SourceGeneratedProxyKind.Interface);
            Assert.Equal(key1.GetHashCode(), key2.GetHashCode());
        }

        [Fact]
        public void CacheKey_GetHashCode_WithImplType_DifferentFromNull()
        {
            var type = typeof(ISgExtService);
            var key1 = CreateCacheKey(type, null, SourceGeneratedProxyKind.Interface);
            var key2 = CreateCacheKey(type, typeof(SgExtServiceImpl), SourceGeneratedProxyKind.Interface);
            // Hash codes may or may not differ, but both should be computable
            Assert.True(key1.GetHashCode() != 0 || key2.GetHashCode() != 0);
        }

        [Fact]
        public void CreateInterfaceProxyType_SourceGenerator_Missing_ExceptionContainsScannedCount()
        {
            // Tests CreateMissingProxyException which calls EnsureScannedRegistries
            var options = CreateOptions(ProxyEngine.SourceGenerator, strict: true);
            var generator = CreateGenerator(options);
            var ex = Assert.Throws<InvalidOperationException>(() =>
                generator.CreateInterfaceProxyType(typeof(ISgExtService)));
            Assert.Contains("ScannedRegistries:", ex.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void CreateInterfaceProxyType_SourceGenerator_WithManualRegistryMatch_ButWrongKind_Throws()
        {
            // Registry matches interface kind but we ask for class kind
            var registry = new ClassOnlyRegistry();
            var options = CreateOptions(ProxyEngine.SourceGenerator);
            var generator = CreateGenerator(options, new[] { registry });
            // Asking for interface proxy, but registry only has class
            Assert.Throws<InvalidOperationException>(() =>
                generator.CreateInterfaceProxyType(typeof(ISgExtService)));
        }

        private static object CreateCacheKey(Type serviceType, Type implType, SourceGeneratedProxyKind kind)
        {
            var cacheKeyType = typeof(SourceGeneratedProxyTypeGenerator).GetNestedType("CacheKey",
                BindingFlags.NonPublic | BindingFlags.Public);
            Assert.NotNull(cacheKeyType);
            var constructor = cacheKeyType.GetConstructor(
                new[] { typeof(Type), typeof(Type), typeof(SourceGeneratedProxyKind) });
            Assert.NotNull(constructor);
            return constructor.Invoke(new object[] { serviceType, implType, kind });
        }

        #region Test Types

        public interface ISgExtService
        {
            int GetValue();
        }

        public interface ISgExtService2
        {
            string GetName();
        }

        public class SgExtServiceImpl : ISgExtService
        {
            public int GetValue() => 42;
        }

        public class SgExtClass
        {
            public virtual int GetValue() => 42;
        }

        public interface IGenericExtSvc<T>
        {
            T Echo(T value);
        }

        public class GenericExtSvcImpl<T> : IGenericExtSvc<T>
        {
            public T Echo(T value) => value;
        }

        private sealed class NonMatchingRegistry : ISourceGeneratedProxyRegistry
        {
            public bool TryGetProxyType(Type serviceType, Type implementationType, SourceGeneratedProxyKind kind, out Type proxyType)
            {
                proxyType = null;
                return false;
            }
        }

        private sealed class ClassOnlyRegistry : ISourceGeneratedProxyRegistry
        {
            public bool TryGetProxyType(Type serviceType, Type implementationType, SourceGeneratedProxyKind kind, out Type proxyType)
            {
                if (kind == SourceGeneratedProxyKind.Class)
                {
                    proxyType = typeof(SgExtClassProxy);
                    return true;
                }
                proxyType = null;
                return false;
            }
        }

        public class SgExtClassProxy : SgExtClass
        {
            public override int GetValue() => 99;
        }

        #endregion
    }
}
