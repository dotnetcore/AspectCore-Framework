using System;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class ScopedParameterTests : DynamicProxyTestBase
    {
        [Fact]
        public void ScopedRef_Parameter_Should_Proxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ScopedService>();
            Assert.True(proxy.IsProxy());

            int value = 42;
            proxy.SetValue(ref value);
            Assert.Equal(42, value);
        }

        [Fact]
        public void ScopedRef_Parameter_Should_Intercept()
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

            var proxy = generator.CreateClassProxy<ScopedService>();
            int value = 10;
            proxy.SetValue(ref value);
            Assert.Equal(1, callCount);
        }

        protected override void Configure(IAspectConfiguration configuration)
        {
            configuration.Interceptors.AddDelegate((ctx, next) => next(ctx));
        }
    }

    public class ScopedService
    {
        public virtual void SetValue(scoped ref int value)
        {
            value = value * 1;
        }
    }
}
