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
                ProxyGenerator.CreateClassProxy<FakeExplicitImplementation>();
            var result = ((IFakeExplicitImplementation)service).GetVal_NonAspect();
            Assert.Equal("lemon", result);
            var result2 = ((IFakeExplicitImplementation)service).GetVal2();
            Assert.Equal(1, result2);
        }
        
        [Fact]
        public void ExplicitImplementation_Aspect_Test()
        {
            var service =
                ProxyGenerator.CreateClassProxy<FakeExplicitImplementation>();
            var result = ((IFakeExplicitImplementation)service).GetVal();
            Assert.Equal("lemon", result);
        }

        protected override void Configure(IAspectConfiguration configuration)
        {
            configuration.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next), Predicates.ForMethod("GetVal"));
            base.Configure(configuration);
        }
    }
}