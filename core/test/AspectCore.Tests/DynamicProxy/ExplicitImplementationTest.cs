using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Tests.DynamicProxy
{
    public class ExplicitImplementationTest : DynamicProxyTestBase
    {
        [Fact]
        public void ExplicitImplementation_NonAspect_Test()
        {
            var service =
                ProxyGenerator.CreateInterfaceProxy<IFakeExplicitImplementation, FakeExplicitImplementation>();
            var result = service.GetVal_NonAspect();
            Assert.Equal("lemon", result);
        }
        
        [Fact]
        public void ExplicitImplementation_Aspect_Test()
        {
            var service =
                ProxyGenerator.CreateInterfaceProxy<IFakeExplicitImplementation, FakeExplicitImplementation>();
            var result = service.GetVal();
            Assert.Equal("lemon", result);
        }

        protected override void Configure(IAspectConfiguration configuration)
        {
            configuration.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next), Predicates.ForMethod("GetVal"));
            base.Configure(configuration);
        }
    }
}