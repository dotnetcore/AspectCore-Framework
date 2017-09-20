using AspectCore.Configuration;
using AspectCore.Injector;
using Xunit;
using AspectCore.DynamicProxy;
using System.Threading.Tasks;

namespace AspectCore.Tests.Integrate
{
    public class Container_Built_In_ProxyTests : IntegrateTestBase
    {
        [Fact]
        public void Interface_Proxy()
        {
            var serviceContainer = new ServiceContainer();
            serviceContainer.AddType<IProxyTransient, ProxyTransient>();
            var resolver = serviceContainer.Build();

            var transient = resolver.Resolve<IProxyTransient>();
            Assert.True(transient.IsProxy());
        }

        [Fact]
        public void Class_Proxy()
        {
            var serviceContainer = new ServiceContainer();
            ConfigureServiceInternal(serviceContainer);
            var resolver = serviceContainer.Build();

            var transient = resolver.Resolve<ProxyTransient>();
            Assert.True(transient.IsProxy());
        }

        protected void ConfigureServiceInternal(IServiceContainer serviceContainer)
        {
            serviceContainer.AddType<IProxyTransient, ProxyTransient>();
            serviceContainer.AddType<ProxyTransient>();
        }

        public class Test : AbstractInterceptorAttribute
        {
            public override Task Invoke(AspectContext context, AspectDelegate next)
            {
                return next(context);
            }
        }

        [Test]
        public interface IProxyTransient
        {
            void Foo();
        }

        [Test]
        public class ProxyTransient : IProxyTransient
        {
            public virtual void Foo()
            {
            }
        }
    }
}