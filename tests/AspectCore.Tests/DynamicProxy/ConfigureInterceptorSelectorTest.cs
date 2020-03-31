using System;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Tests.DynamicProxy
{
    public class ConfigureInterceptorSelectorTest : DynamicProxyTestBase
    {
        [Fact]
        public void Selector_Test()
        {
            var service = ProxyGenerator.CreateClassProxy<FakeSelectService>();
            var result = service.GetVal(0);
            Assert.Equal(3, result);
        }
        
        protected override void Configure(IAspectConfiguration configuration)
        {
            configuration.Interceptors.AddTyped<Intercept1>(Predicates.ForNameSpace("*"));
            configuration.Interceptors.AddTyped<Intercept2>();
        }

        private class Intercept1 : AbstractInterceptor
        {
            public override async Task Invoke(AspectContext context, AspectDelegate next)
            {
                await context.Invoke(next);
                var result = (int) context.ReturnValue;
                context.ReturnValue = result + 1;
            }
        }

        private class Intercept2 : AbstractInterceptor
        {
            public override async Task Invoke(AspectContext context, AspectDelegate next)
            {
                await context.Invoke(next);
                var result = (int) context.ReturnValue;
                context.ReturnValue = result + 2;
            }
        }
    }
}

namespace AspectCore.Tests.DynamicProxy
{
    public class FakeSelectService
    {
        public virtual int GetVal(int val)
        {
            return val;
        }
    }
}

