using System;
using System.Collections.Generic;
using System.Linq;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class ParamsCollectionTests : DynamicProxyTestBase
    {
        [Fact]
        public void ParamsIEnumerable_Should_Proxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ParamsCollectionService>();
            Assert.True(proxy.IsProxy());

            // Call with params syntax (multiple arguments)
            var result = proxy.Sum(1, 2, 3, 4, 5);
            Assert.Equal(15, result);

            // Call with single IEnumerable argument
            var result2 = proxy.Sum(new[] { 10, 20, 30 });
            Assert.Equal(60, result2);
        }

        [Fact]
        public void ParamsArray_Should_Proxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ParamsCollectionService>();
            Assert.True(proxy.IsProxy());

            var result = proxy.Concat("a", "b", "c");
            Assert.Equal("a,b,c", result);

            var result2 = proxy.Concat(new[] { "x", "y" });
            Assert.Equal("x,y", result2);
        }

#if NET10_0_OR_GREATER
        [Fact]
#else
        [Fact(Skip = "Requires .NET 10+ for ParamCollectionAttribute")]
#endif
        public void ParamsCollection_Should_HaveCompilerGenerated_ParamCollectionAttribute()
        {
#if NET10_0_OR_GREATER
            // ParamCollectionAttribute was introduced in .NET 10 (C# 13).
            // On older target frameworks the attribute does not exist, so the
            // params-collection parameter carries no attribute to forward.
            var proxy = ProxyGenerator.CreateClassProxy<ParamsCollectionService>();
            var method = proxy.GetType().GetMethod("Sum");
            Assert.NotNull(method);

            var param = method!.GetParameters()[0];
            var hasParamCollection = param.GetCustomAttributes(false)
                .Any(a => a.GetType().Name == "ParamCollectionAttribute");
            Assert.True(hasParamCollection, "The compiler should synthesize ParamCollectionAttribute from the params keyword");
#endif
        }

        [Fact]
        public void InterfaceProxy_ParamsIEnumerable_Should_Proxy()
        {
            var implementation = new ParamsCollectionImpl();
            var proxy = ProxyGenerator.CreateInterfaceProxy<IParamsCollectionService>(implementation);
            Assert.True(proxy.IsProxy());

            // Call with params syntax (multiple arguments)
            var result = proxy.Sum(1, 2, 3, 4, 5);
            Assert.Equal(15, result);

            // Call with single IEnumerable argument
            var result2 = proxy.Sum(new[] { 10, 20, 30 });
            Assert.Equal(60, result2);
        }

        [Fact]
        public void InterfaceProxy_ParamsArray_Should_Proxy()
        {
            var implementation = new ParamsCollectionImpl();
            var proxy = ProxyGenerator.CreateInterfaceProxy<IParamsCollectionService>(implementation);
            Assert.True(proxy.IsProxy());

            var result = proxy.Concat("a", "b", "c");
            Assert.Equal("a,b,c", result);

            var result2 = proxy.Concat(new[] { "x", "y" });
            Assert.Equal("x,y", result2);
        }

        [Fact]
        public void InterfaceProxy_Params_WithInterceptor_Should_Intercept()
        {
            int callCount = 0;
            var generator = new ProxyGeneratorBuilder()
                .Configure(cfg =>
                {
                    cfg.Interceptors.AddDelegate((ctx, next) =>
                    {
                        callCount++;
                        return next(ctx);
                    });
                })
                .Build();
            var proxy = generator.CreateInterfaceProxy<IParamsCollectionService>(new ParamsCollectionImpl());

            var result = proxy.Sum(1, 2, 3);
            Assert.Equal(6, result);
            Assert.Equal(1, callCount);
        }

        protected override void Configure(IAspectConfiguration configuration)
        {
            configuration.Interceptors.AddDelegate((ctx, next) =>
            {
                return next(ctx);
            });
        }
    }

    public class ParamsCollectionService
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

    public interface IParamsCollectionService
    {
        int Sum(params IEnumerable<int> values);
        string Concat(params string[] items);
    }

    public class ParamsCollectionImpl : IParamsCollectionService
    {
        public int Sum(params IEnumerable<int> values) => values.Sum();
        public string Concat(params string[] items) => string.Join(",", items);
    }
}
