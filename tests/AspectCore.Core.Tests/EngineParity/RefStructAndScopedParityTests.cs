using System;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.EngineParity
{
    public class RefStructAndScopedParityTests
    {
        [Theory]
        [MemberData(nameof(Engines))]
        public void ScopedRef_Parameter_Should_Work(ProxyEngine engine)
        {
            using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(engine, cfg =>
            {
                cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next));
            });

            var proxy = proxyGenerator.CreateClassProxy<ScopedParityService>();
            Assert.True(proxy.IsProxy());

            int value = 42;
            proxy.SetValue(ref value);
            Assert.Equal(42, value);
        }

        [Theory]
        [MemberData(nameof(Engines))]
        public void ScopedRef_Parameter_With_Interceptor_Should_Intercept(ProxyEngine engine)
        {
            int callCount = 0;
            using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(engine, cfg =>
            {
                cfg.Interceptors.AddDelegate((ctx, next) =>
                {
                    callCount++;
                    return ctx.Invoke(next);
                });
            });

            var proxy = proxyGenerator.CreateClassProxy<ScopedParityService>();
            int value = 10;
            proxy.SetValue(ref value);
            Assert.Equal(1, callCount);
        }

#if NET10_0_OR_GREATER
        [Theory]
        [MemberData(nameof(Engines))]
        public void ScopedIn_Parameter_Should_Work(ProxyEngine engine)
        {
            using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(engine, cfg =>
            {
                cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next));
            });

            var proxy = proxyGenerator.CreateClassProxy<ScopedParityService>();
            Assert.True(proxy.IsProxy());

            var result = proxy.Compute(5);
            Assert.Equal(25, result);
        }
#endif

        public static TheoryData<ProxyEngine> Engines => new()
        {
            ProxyEngine.DynamicProxy,
            ProxyEngine.SourceGenerator,
        };
    }

    [AspectCoreGenerateProxy]
    public class ScopedParityService
    {
        public virtual void SetValue(scoped ref int value)
        {
            value = value * 1;
        }
#if NET10_0_OR_GREATER
        public virtual int Compute(scoped in int value)
        {
            return value * value;
        }
#endif
    }
}
