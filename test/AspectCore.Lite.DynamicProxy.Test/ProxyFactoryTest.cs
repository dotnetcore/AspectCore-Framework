using AspectCore.Lite.DynamicProxy.Test.Fakes;
using Xunit;

namespace AspectCore.Lite.DynamicProxy.Test
{
    public class ProxyFactoryTest
    {
        [Fact]
        public void Interface_Aspect_Method_Proxy_Test()
        {
            IProxyFactory proxyFactory = new ProxyFactoryBuilder().Build();
            var proxy = (IAppService)proxyFactory.CreateProxy(typeof(IAppService), typeof(AppService));
            Assert.IsAssignableFrom<IAppService>(proxy);
            Assert.Equal(proxy.Run(1), 2);
        }

        [Fact]
        public void Interface_Aspect_Generic_Method_Proxy_Test()
        {
            IProxyFactory proxyFactory = new ProxyFactoryBuilder().Build();
            var proxy = (IAppService)proxyFactory.CreateProxy(typeof(IAppService), typeof(AppService));
            Assert.IsAssignableFrom<IAppService>(proxy);
            Assert.Equal(proxy.Run1<int>(1), 2);
        }

        [Fact]
        public void Interface_NonAspect_Method_Proxy_Test()
        {
            IProxyFactory proxyFactory = new ProxyFactoryBuilder().Build();
            var proxy = (IAppService1)proxyFactory.CreateProxy(typeof(IAppService1), typeof(AppService));
            Assert.IsAssignableFrom<IAppService1>(proxy);
            Assert.Equal(proxy.Run(1), 1);
        }

        [Fact]
        public void Interface_NonAspect_Generic_Method_Proxy_Test()
        {
            IProxyFactory proxyFactory = new ProxyFactoryBuilder().Build();
            var proxy = (IAppService1)proxyFactory.CreateProxy(typeof(IAppService1), typeof(AppService));
            Assert.IsAssignableFrom<IAppService1>(proxy);
            Assert.Equal(proxy.Run1<int>(1), 1);
        }

        [Fact]
        public void Generic_Interface_Aspect_Method_Proxy_Test()
        {
            IProxyFactory proxyFactory = new ProxyFactoryBuilder().Build();
            var proxy = (IAppService<int>)proxyFactory.CreateProxy(typeof(IAppService<int>), typeof(AppService<int>));
            Assert.IsAssignableFrom<IAppService<int>>(proxy);
            Assert.Equal(proxy.Run(1), 2);
        }

        [Fact]
        public void Generic_Interface_Aspect_Generic_Method_Proxy_Test()
        {
            IProxyFactory proxyFactory = new ProxyFactoryBuilder().Build();
            var proxy = (IAppService<int>)proxyFactory.CreateProxy(typeof(IAppService<int>), typeof(AppService<int>));
            Assert.IsAssignableFrom<IAppService<int>>(proxy);
            Assert.Equal(proxy.Run1(1), 2);
        }

        [Fact]
        public void Generic_Interface_NonAspect_Method_Proxy_Test()
        {
            IProxyFactory proxyFactory = new ProxyFactoryBuilder().Build();
            var proxy = (IAppService1<int>)proxyFactory.CreateProxy(typeof(IAppService1<int>), typeof(AppService<int>));
            Assert.IsAssignableFrom<IAppService1<int>>(proxy);
            Assert.Equal(proxy.Run(1), 1);
        }

        [Fact]
        public void Generic_Interface_NonAspect_Generic_Method_Proxy_Test()
        {
            IProxyFactory proxyFactory = new ProxyFactoryBuilder().Build();
            var proxy = (IAppService1<int>)proxyFactory.CreateProxy(typeof(IAppService1<int>), typeof(AppService<int>));
            Assert.IsAssignableFrom<IAppService1<int>>(proxy);
            Assert.Equal(proxy.Run1(1), 1);
        }

        [Fact]
        public void Class_Proxy_Test()
        {
            IProxyFactory proxyFactory = new ProxyFactoryBuilder().Build();
            var proxy = (AbsAppService)proxyFactory.CreateProxy(typeof(AbsAppService), typeof(AppService));
            Assert.IsAssignableFrom<AbsAppService>(proxy);
            Assert.Equal(proxy.Run1(2), 1);
        }
    }
}
