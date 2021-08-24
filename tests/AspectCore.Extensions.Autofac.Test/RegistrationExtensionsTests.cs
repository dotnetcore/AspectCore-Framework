﻿using System.Reflection;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Autofac;
using AspectCore.Extensions.Test.Fakes;
using Autofac;
using Xunit;

namespace AspectCoreTest.Autofac
{
    public class RegistrationExtensionsTests
    {
        private ContainerBuilder CreateBuilder()
        {
            return new ContainerBuilder().RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate(next => ctx => next(ctx),Predicates.ForNameSpace("AspectCore.Extensions.Test.Fakes"));
            });
        }

        [Fact]
        public void AsProxy_Test()
        {
            var builder = CreateBuilder();
            builder.RegisterType<Service>().As<IService>();
            var container = builder.Build();
            var proxyService = container.Resolve<IService>();
            Assert.True(proxyService.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
            Assert.Equal(proxyService.Get(1), proxyService.Get(1));
        }

        [Fact]
        public void AsNamedProxy_Test()
        {
            var builder = CreateBuilder();
            builder.RegisterType<Service>().Named<IService>("proxy");
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
            builder.RegisterType<Service>().Keyed<IService>(key);
            var container = builder.Build();
            var proxyService = container.ResolveKeyed<IService>(key);
            Assert.True(proxyService.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
            Assert.Equal(proxyService.Get(1), proxyService.Get(1));
        }

        [Fact]
        public void AsProxyWithParamter_Test()
        {
            var builder = CreateBuilder();
            builder.RegisterType<Service>().As<IService>();
            builder.RegisterType<Controller>().As<IController>();
            var container = builder.Build();

            var proxyService = container.Resolve<IService>();
            Assert.True(proxyService.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
            Assert.Equal(proxyService.Get(1), proxyService.Get(1));

            var proxyController = container.Resolve<IController>();
            Assert.Equal(proxyService.Get(100), proxyController.Execute());
        }

        [Fact]
        public void Intercept_OutWithDecimalParamter_Test()
        {
            var builder = CreateBuilder();
            builder.RegisterType<FakeServiceWithOut>().As<IFakeServiceWithOut>();
            var container = builder.Build();

            var proxyServiceWithOut = container.Resolve<IFakeServiceWithOut>();
            decimal num;
            Assert.True(proxyServiceWithOut.OutDecimal(out num));
            Assert.Equal(1.0M, num);
        }

        [Fact]
        public void Intercept_OutWithIntParamter_Test()
        {
            var builder = CreateBuilder();
            builder.RegisterType<FakeServiceWithOut>().As<IFakeServiceWithOut>();
            var container = builder.Build();

            var proxyServiceWithOut = container.Resolve<IFakeServiceWithOut>();
            int num;
            Assert.True(proxyServiceWithOut.OutInt(out num));
            Assert.Equal(1, num);
        }
    }
}
