using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.Injector;
using Xunit;

namespace AspectCore.Tests.Integrate
{
    public class ServiceInterceptorTests : IntegrateTestBase
    {
        [Fact]
        public void Service_Interceptor_Tests()
        {
            var service = ServiceResolver.Resolve<IProxyTransient>();
            Assert.Equal(1, service.Foo());
        }

        protected override void ConfigureService(IServiceContainer serviceContainer)
        {
            serviceContainer.AddType<Test>();
            serviceContainer.AddType<IProxyTransient,ProxyTransient>();
        }

        public class Test : AbstractInterceptor
        {
            public override async Task Invoke(AspectContext context, AspectDelegate next)
            {
                await next(context);
                var val = (int) context.ReturnValue;
                context.ReturnValue = val + 1;
            }
        }

        [ServiceInterceptor(typeof(Test))]
        public interface IProxyTransient
        {
            int Foo();
        }

        [ServiceInterceptor(typeof(Test))]
        public class ProxyTransient : IProxyTransient
        {
            public virtual int Foo()
            {
                return 0;
            }
        }
    }
}
