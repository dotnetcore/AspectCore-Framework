using AspectCore.Abstractions;
using System;
using Xunit;
using Autofac;
using AspectCore.Extensions.Test.Fakes;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.Autofac.Test
{
    public class RegistrationExtensionsTests
    {
        private ContainerBuilder CreateBuilder()
        {
            return new ContainerBuilder().RegisterAspectCore();
        }

        [Fact]
        public void AsProxy_Test()
        {
            var builder = CreateBuilder();
            builder.RegisterType<Service>().As<IService>().AsInterfacesProxy();
            var container = builder.Build();
            var proxyService = container.Resolve<IService>();
            Assert.True(proxyService.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
            Assert.Equal(proxyService.Get(1), proxyService.Get(1));
        }

        [Fact]
        public void AsNamedProxy_Test()
        {
            var builder = CreateBuilder();
            builder.RegisterType<Service>().Named<IService>("proxy").AsInterfacesProxy();
            var container = builder.Build();
            var proxyService = container.ResolveNamed<IService>("proxy");
            Assert.True(proxyService.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
            Assert.Equal(proxyService.Get(1), proxyService.Get(1));
        }

        [Fact]
        public void AsKeyedProxy_Test()
        {
            var builder = CreateBuilder();
            var key = new object();
            builder.RegisterType<Service>().Keyed<IService>(key).AsInterfacesProxy();
            var container = builder.Build();
            var proxyService = container.ResolveKeyed<IService>(key);
            Assert.True(proxyService.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
            Assert.Equal(proxyService.Get(1), proxyService.Get(1));
        }

        [Fact]
        public void AsProxyWithParamter_Test()
        {
            var builder = CreateBuilder();
            builder.RegisterType<Service>().As<IService>().AsInterfacesProxy();
            builder.RegisterType<Controller>().As<IController>().AsInterfacesProxy();
            var container = builder.Build();

            var proxyService = container.Resolve<IService>();
            Assert.True(proxyService.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
            Assert.Equal(proxyService.Get(1), proxyService.Get(1));

            var proxyController = container.Resolve<IController>();
            Assert.Equal(proxyService.Get(100), proxyController.Execute());
        }

        [Fact]
        public void OriginalServiceProvider_Test()
        {
            var builder = CreateBuilder();
            builder.RegisterType<Service>().As<IService>().AsInterfacesProxy();
            var container = builder.Build();
            var proxyService = container.Resolve<IRealServiceProvider>().GetService<IService>();
            Assert.False(proxyService.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
            Assert.NotEqual(proxyService.Get(1), proxyService.Get(1));
        }

        [Fact]
        public void OriginalServiceProviderWithParameter_Test()
        {
            var builder = CreateBuilder();
            builder.RegisterType<Service>().As<IService>().AsInterfacesProxy();
            builder.RegisterType<Controller>().As<IController>().AsInterfacesProxy();

            var container = builder.Build();

            var proxyService = container.Resolve<IRealServiceProvider>().GetService<IService>();

            Assert.False(proxyService.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));

            var proxyController = container.Resolve<IRealServiceProvider>().GetService<IController>();

            Assert.False(proxyController.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
            Assert.False(proxyController.Service.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));

            Assert.NotEqual(proxyService.Get(100), proxyController.Execute());
        }
    }
}
