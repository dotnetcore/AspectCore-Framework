using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.LightInject;
using LightInject;
using Xunit;

namespace AspectCoreTest.LightInject
{
   public class AdditionalInterceptorSelectorTests
    {
        [Fact]
        public void ImplementationMethod_Test()
        {
            var builder = new ServiceContainer();
            builder.RegisterDynamicProxy();
            builder.Register<IService, Service>();
            var provider = builder;
            var service = provider.GetInstance<IService>();
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
