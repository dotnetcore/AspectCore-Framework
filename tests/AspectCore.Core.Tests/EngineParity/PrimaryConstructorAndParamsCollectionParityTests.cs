using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.EngineParity
{
    public class PrimaryConstructorAndParamsCollectionParityTests
    {
        [Theory]
        [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
        public void ClassPrimaryConstructor_Should_Work(ProxyEngine engine)
        {
            using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
                engine,
                configureAspect: cfg =>
                {
                    cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next));
                },
                strict: engine == ProxyEngine.SourceGenerator,
                allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

            var proxy = proxyGenerator.CreateClassProxy<PrimaryConstructorParityService>(42, "hello");
            Assert.True(proxy.IsProxy());
            Assert.Equal(42, proxy.Count);
            Assert.Equal("hello", proxy.Name);
            Assert.Equal(84, proxy.DoubleCount());
        }

        [Theory]
        [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
        public void RecordStylePrimaryConstructor_Should_Work(ProxyEngine engine)
        {
            using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
                engine,
                configureAspect: cfg =>
                {
                    cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next));
                },
                strict: engine == ProxyEngine.SourceGenerator,
                allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

            var proxy = proxyGenerator.CreateClassProxy<RecordStylePrimaryConstructorParityService>(99, "world");
            Assert.True(proxy.IsProxy());
            Assert.Equal(99, proxy.Value);
            Assert.Equal("world", proxy.Label);
            Assert.Equal("world-99", proxy.GetFormatted());
        }

        [Theory]
        [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
        public void ParamsIEnumerable_Should_Work(ProxyEngine engine)
        {
            using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
                engine,
                configureAspect: cfg =>
                {
                    cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next));
                },
                strict: engine == ProxyEngine.SourceGenerator,
                allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

            var proxy = proxyGenerator.CreateClassProxy<ParamsCollectionParityService>();
            Assert.True(proxy.IsProxy());

            // Call with params syntax (multiple arguments)
            var result = proxy.Sum(1, 2, 3, 4, 5);
            Assert.Equal(15, result);

            // Call with single collection argument
            var result2 = proxy.Sum(new[] { 10, 20, 30 });
            Assert.Equal(60, result2);
        }

        [Theory]
        [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
        public void ParamsArray_Should_Work(ProxyEngine engine)
        {
            using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
                engine,
                configureAspect: cfg =>
                {
                    cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next));
                },
                strict: engine == ProxyEngine.SourceGenerator,
                allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

            var proxy = proxyGenerator.CreateClassProxy<ParamsCollectionParityService>();
            Assert.True(proxy.IsProxy());

            var result = proxy.Concat("a", "b", "c");
            Assert.Equal("a,b,c", result);

            var result2 = proxy.Concat(new[] { "x", "y" });
            Assert.Equal("x,y", result2);
        }

        [Theory]
        [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
        public void PrimaryConstructor_WithInterceptor_Should_Intercept(ProxyEngine engine)
        {
            int callCount = 0;
            using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
                engine,
                configureAspect: cfg =>
                {
                    cfg.Interceptors.AddDelegate((ctx, next) =>
                    {
                        callCount++;
                        return ctx.Invoke(next);
                    });
                },
                strict: engine == ProxyEngine.SourceGenerator,
                allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

            var proxy = proxyGenerator.CreateClassProxy<PrimaryConstructorParityService>(5, "test");
            Assert.Equal(10, proxy.DoubleCount());
            Assert.Equal(1, callCount);
        }

        [Theory]
        [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
        public void ParamsCollection_WithInterceptor_Should_Intercept(ProxyEngine engine)
        {
            int callCount = 0;
            using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
                engine,
                configureAspect: cfg =>
                {
                    cfg.Interceptors.AddDelegate((ctx, next) =>
                    {
                        callCount++;
                        return ctx.Invoke(next);
                    });
                },
                strict: engine == ProxyEngine.SourceGenerator,
                allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

            var proxy = proxyGenerator.CreateClassProxy<ParamsCollectionParityService>();
            var result = proxy.Sum(1, 2, 3);
            Assert.Equal(6, result);
            Assert.Equal(1, callCount);
        }

        [Theory]
        [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
        public void PrimaryConstructor_WithParamsCollection_Should_Work(ProxyEngine engine)
        {
            using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
                engine,
                configureAspect: cfg =>
                {
                    cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next));
                },
                strict: engine == ProxyEngine.SourceGenerator,
                allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

            var proxy = proxyGenerator.CreateClassProxy<PrimaryConstructorWithParamsCollectionService>(
                3,
                (IEnumerable<int>)new[] { 2, 4, 6 });

            Assert.True(proxy.IsProxy());
            Assert.Equal(3, proxy.Multiplier);
            Assert.Equal(36, proxy.Total);
            Assert.Equal(72, proxy.DoubleTotal());
        }

        [Theory]
        [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
        public void ClassConstructor_WithParamsIEnumerable_Should_Work(ProxyEngine engine)
        {
            using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
                engine,
                configureAspect: cfg =>
                {
                    cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next));
                },
                strict: engine == ProxyEngine.SourceGenerator,
                allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

            var proxy = proxyGenerator.CreateClassProxy<ParamsIEnumerableConstructorService>(
                (IEnumerable<int>)new[] { 1, 2, 3, 4 });

            Assert.True(proxy.IsProxy());
            Assert.Equal(10, proxy.Total);
            Assert.Equal(20, proxy.DoubleTotal());
        }

        [Theory]
        [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
        public void ClassConstructor_WithParamsArray_Should_Work(ProxyEngine engine)
        {
            using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
                engine,
                configureAspect: cfg =>
                {
                    cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next));
                },
                strict: engine == ProxyEngine.SourceGenerator,
                allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

            var proxy = proxyGenerator.CreateClassProxy<ParamsArrayConstructorService>(
                new object[] { new[] { "a", "b", "c" } });

            Assert.True(proxy.IsProxy());
            Assert.Equal("a,b,c", proxy.Joined);
            Assert.Equal("a,b,c!", proxy.Format());
        }

        [Theory]
        [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
        public void ClassConstructor_WithLeadingParameterAndParams_Should_Work(ProxyEngine engine)
        {
            using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
                engine,
                configureAspect: cfg =>
                {
                    cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next));
                },
                strict: engine == ProxyEngine.SourceGenerator,
                allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

            var proxy = proxyGenerator.CreateClassProxy<MixedParamsConstructorService>(
                "prefix",
                (IEnumerable<int>)new[] { 2, 4, 6 });

            Assert.True(proxy.IsProxy());
            Assert.Equal("prefix:12", proxy.Value);
            Assert.Equal("prefix:12!", proxy.Format());
        }
    }

    [AspectCoreGenerateProxy]
    public class PrimaryConstructorParityService(int count, string name)
    {
        public int Count => count;
        public string Name => name;
        public virtual int DoubleCount() => count * 2;
    }

    [AspectCoreGenerateProxy]
    public class RecordStylePrimaryConstructorParityService(int value, string label)
    {
        public int Value => value;
        public string Label => label;
        public virtual string GetFormatted() => $"{label}-{value}";
    }

    [AspectCoreGenerateProxy]
    public class ParamsCollectionParityService
    {
        public virtual int Sum(params IEnumerable<int> values)
        {
            return values.Sum();
        }

        public virtual string Concat(params string[] items)
        {
            return string.Join(",", items);
        }
    }

    [AspectCoreGenerateProxy]
    public class PrimaryConstructorWithParamsCollectionService(int multiplier, params IEnumerable<int> values)
    {
        public int Multiplier => multiplier;
        public int Total => values.Sum() * multiplier;
        public virtual int DoubleTotal() => Total * 2;
    }

    [AspectCoreGenerateProxy]
    public class ParamsIEnumerableConstructorService
    {
        public ParamsIEnumerableConstructorService(params IEnumerable<int> values)
        {
            Total = values.Sum();
        }

        public int Total { get; }

        public virtual int DoubleTotal() => Total * 2;
    }

    [AspectCoreGenerateProxy]
    public class ParamsArrayConstructorService
    {
        public ParamsArrayConstructorService(params string[] items)
        {
            Joined = string.Join(",", items);
        }

        public string Joined { get; }

        public virtual string Format() => Joined + "!";
    }

    [AspectCoreGenerateProxy]
    public class MixedParamsConstructorService
    {
        public MixedParamsConstructorService(string prefix, params IEnumerable<int> values)
        {
            Value = prefix + ":" + values.Sum();
        }

        public string Value { get; }

        public virtual string Format() => Value + "!";
    }
}
