using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.DependencyInjection;
using Xunit;

namespace AspectCore.Tests.Integrate
{
    public class ServiceInterceptorTests : IntegrateTestBase
    {
        [Fact]
        public void Service_Interceptor_AllowMultiple_Tests()
        {
            var service = ServiceResolver.Resolve<IProxyTransient>();
            // ServiceInterceptorAttribute.AllowMultiple = true.
            // Both [ServiceInterceptor(typeof(Test))] on interface and class should execute.
            // Before fix: .Distinct() deduplicated them (same _interceptorType) → result = 1.
            // After fix: both execute → result = 2.
            Assert.Equal(2, service.Foo());
        }

        [Fact]
        public void Service_Interceptor_Single_Attribute_Tests()
        {
            var service = ServiceResolver.Resolve<IProxySingle>();
            // Only class has [ServiceInterceptor], interface has none.
            // Single interceptor executes → result = 1.
            Assert.Equal(1, service.Bar());
        }

        protected override void ConfigureService(IServiceContext serviceContext)
        {
            serviceContext.AddType<Test>();
            serviceContext.AddType<IProxyTransient, ProxyTransient>();
            serviceContext.AddType<IProxySingle, ProxySingle>();
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

        public interface IProxySingle
        {
            int Bar();
        }

        [ServiceInterceptor(typeof(Test))]
        public class ProxySingle : IProxySingle
        {
            public virtual int Bar()
            {
                return 0;
            }
        }
    }
}
