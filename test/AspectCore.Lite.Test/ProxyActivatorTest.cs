using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Abstractions.Activators;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AspectCore.Lite.Test
{
    public class ProxyActivatorTest : IDependencyInjection
    {
        [Fact]
        public void CreateInterfaceProxyWithOutInterceptor_Test()
        {
            IService1 service= Substitute.For<IService1>();
            service.Run().Returns(service);
            IProxyActivator proxyActivator = new ProxyActivator();
            IService1 serviceProxy = (IService1)proxyActivator.CreateInterfaceProxy(typeof(IService1), service);
            Assert.IsAssignableFrom<IService1>(serviceProxy);
            Assert.NotEqual(serviceProxy, service);
            Assert.Equal(service.Run(), serviceProxy.Run());
        }

        [Fact]
        public void CreateInterfaceProxyWithInterceptor_Test()
        {
            IService2 service = Substitute.For<IService2>();
            service.Run().Returns(service);
            IProxyActivator proxyActivator = new ProxyActivator();
            IService2 serviceProxy = (IService2)proxyActivator.CreateInterfaceProxy(typeof(IService2), service);
            Assert.IsAssignableFrom<IService2>(serviceProxy);
            Assert.NotEqual(serviceProxy, service);
            Assert.NotEqual(service.Run(), serviceProxy.Run());
            Assert.IsType<Interceptor1Attribute>(serviceProxy.Run());
        }

        [Fact]
        public void CreateClassProxyWithOutInterceptor_Test()
        {
            Service1 service = new Service1();
            IProxyActivator proxyActivator = new ProxyActivator();
            Service1 serviceProxy = (Service1)proxyActivator.CreateClassProxy(typeof(Service1), service);
            Assert.IsAssignableFrom<Service1>(serviceProxy);
            Assert.NotEqual(serviceProxy, service);
            Assert.Equal(service.Run(), serviceProxy.Run());
        }

        [Fact]
        public void CreateClassProxyWithInterceptor_Test()
        {
            Service2 service = new Service2();
            IProxyActivator proxyActivator = new ProxyActivator();
            Service2 serviceProxy = (Service2)proxyActivator.CreateClassProxy(typeof(Service2), service);
            Assert.IsAssignableFrom<Service2>(serviceProxy);
            Assert.NotEqual(serviceProxy, service);
            Assert.NotEqual(service.Run(), serviceProxy.Run());
            Assert.IsType<Interceptor1Attribute>(serviceProxy.Run());
        }

        public interface IService1
        {
            object Run();
        }

        public interface IService2
        {
            [Interceptor1]
            object Run();
        }

        public class Service1
        {
            public object Run()
            {
                return "service1";
            }
        }

        public class Service2
        {
            [Interceptor1]
            public virtual object Run()
            {
                return this;
            }
        }

        public class Interceptor1Attribute : InterceptorAttribute
        {
            public override async Task ExecuteAsync(IAspectContext aspectContext, InterceptorDelegate next)
            {
                await base.ExecuteAsync(aspectContext, next);
                aspectContext.ReturnParameter.Value = this;
            }
        }

    }
}
