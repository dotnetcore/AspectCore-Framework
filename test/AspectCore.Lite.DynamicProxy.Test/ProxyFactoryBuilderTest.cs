using NSubstitute;
using System;
using System.Reflection;
using Xunit;

namespace AspectCore.Lite.DynamicProxy.Test
{
    public class ProxyFactoryBuilderTest
    {
        [Fact]
        public void Build_Test()
        {
            ProxyFactoryBuilder builder = new ProxyFactoryBuilder();
            IProxyFactory factory = builder.Build();
            Assert.NotNull(factory);
            Assert.NotNull(factory.ServiceProvider);
            Type serviceproviderType = typeof(ProxyFactoryBuilder).GetTypeInfo().Assembly.GetType("AspectCore.Lite.DynamicProxy.ServiceProvider");
            Assert.IsType(serviceproviderType, factory.ServiceProvider);
        }

        [Fact]
        public void UserServiceProvider_Test()
        {
            IServiceProvider serviceprovider = Substitute.For<IServiceProvider>();
            ProxyFactoryBuilder builder = new ProxyFactoryBuilder().UseServiceProvider(serviceprovider);
            IProxyFactory factory = builder.Build();
            Assert.NotNull(factory);
            Assert.NotNull(factory.ServiceProvider);
            Type serviceproviderType = typeof(ProxyFactoryBuilder).GetTypeInfo().Assembly.GetType("AspectCore.Lite.DynamicProxy.ServiceProvider");
            Assert.IsNotType(serviceproviderType, factory.ServiceProvider);
        }
    }
}
