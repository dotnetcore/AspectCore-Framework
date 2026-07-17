using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class PrimaryConstructorTests : DynamicProxyTestBase
    {
        [Fact]
        public void ClassPrimaryConstructor_Should_Proxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<PrimaryConstructorService>(42, "hello");
            Assert.True(proxy.IsProxy());
            Assert.Equal(42, proxy.Count);
            Assert.Equal("hello", proxy.Name);
            Assert.Equal(84, proxy.DoubleCount());
        }

        [Fact]
        public void RecordStylePrimaryConstructor_Should_Proxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<RecordStylePrimaryConstructorService>(99, "world");
            Assert.True(proxy.IsProxy());
            Assert.Equal(99, proxy.Value);
            Assert.Equal("world", proxy.Label);
            Assert.Equal("world-99", proxy.GetFormatted());
        }

        [Fact]
        public void PrimaryConstructor_Should_Not_Copy_PrimaryConstructorParametersAttribute()
        {
            var proxy = ProxyGenerator.CreateClassProxy<PrimaryConstructorService>(1, "test");
            var proxyType = proxy.GetType();
            var ctorAttrs = proxyType.GetConstructors()
                .SelectMany(c => c.GetCustomAttributes(false))
                .Select(a => a.GetType().Name);
            Assert.DoesNotContain("PrimaryConstructorParametersAttribute", ctorAttrs);
        }

        protected override void Configure(IAspectConfiguration configuration)
        {
            configuration.Interceptors.AddDelegate((ctx, next) =>
            {
                return next(ctx);
            });
        }
    }

    // C# 12 class primary constructor
    public class PrimaryConstructorService(int count, string name)
    {
        public int Count => count;
        public string Name => name;
        public virtual int DoubleCount() => count * 2;
    }

    // Primary constructor on a plain class (record-style syntax, but not a record).
    // Named "RecordStyle" to clarify this uses primary constructor syntax on a
    // non-record type. A real C# 9 record (with auto-generated Equals, GetHashCode,
    // ToString, Deconstruct, and <Clone>$ members) is tested separately and may
    // require additional handling for those compiler-generated members.
    public class RecordStylePrimaryConstructorService(int value, string label)
    {
        public int Value => value;
        public string Label => label;
        public virtual string GetFormatted() => $"{label}-{value}";
    }
}
