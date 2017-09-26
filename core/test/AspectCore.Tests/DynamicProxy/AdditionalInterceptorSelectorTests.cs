using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Tests.DynamicProxy
{
    public class AdditionalInterceptorSelectorTests : DynamicProxyTestBase
    {
        [Fact]
        public void ImplementationMethod_Test()
        {
            var proxy = ProxyGenerator.CreateInterfaceProxy<IService, Service>();
            var val = proxy.GetValue("le");
            Assert.Equal("lemon", val);
        }

        public class Intercept : AbstractInterceptorAttribute
        {
            public override Task Invoke(AspectContext context, AspectDelegate next)
            {
                context.Parameters[0] = "lemon";
                return context.Invoke(next);
            }
        }

        public interface IService
        {
            string GetValue(string val);
        }

        public class Service : IService
        {
            [Intercept]
            public string GetValue(string val)
            {
                return val;
            }
        }
    }
}