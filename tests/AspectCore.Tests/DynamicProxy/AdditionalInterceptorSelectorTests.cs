using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Xunit;
using AspectCore.DependencyInjection;

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

        [Fact]
        public void ImplementationMethodFromInjector_Test()
        {
            var container = new ServiceContext();
            container.AddType<IService, Service>();
            var resolver = container.Build();
            var service = resolver.Resolve<IService>();
            var val = service.GetValue("le");
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