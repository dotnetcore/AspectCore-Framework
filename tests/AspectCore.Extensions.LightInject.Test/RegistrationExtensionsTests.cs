using System.Reflection;
using AspectCore.DynamicProxy;
using AspectCore.Configuration;
using AspectCore.Extensions.LightInject;
using AspectCoreTest.LightInject.Fakes;
using LightInject;
using Xunit;

namespace AspectCoreTest.LightInject
{
    public class RegistrationExtensionsTests
    {
        private IServiceContainer CreateBuilder()
        {
            return new ServiceContainer().RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate(next => ctx => next(ctx), Predicates.ForNameSpace("AspectCore.Extensions.Test.Fakes"));
            });
        }

        [Fact]
        public void AsProxy_Test()
        {
            var builder = CreateBuilder();
            builder.Register<IService, Service>();
            var container = builder;
            var proxyService = container.GetInstance<IService>();
            Assert.True(proxyService.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
            Assert.Equal(proxyService.Get(1), proxyService.Get(1));
        }

        [Fact]
        public void AsNamedProxy_Test()
        {
            var builder = CreateBuilder();
            builder.Register<IService, Service>("proxy");
            var container = builder;
            var proxyService = container.GetInstance<IService>("proxy");
            Assert.True(proxyService.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
            Assert.Equal(proxyService.Get(1), proxyService.Get(1));
        }


        [Fact]
        public void AsProxyWithParamter_Test()
        {
            var builder = CreateBuilder();
            builder.Register<IService, Service>();
            builder.Register<IController, Controller>();
            var container = builder;

            var proxyService = container.GetInstance<IService>();
            Assert.True(proxyService.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
            Assert.Equal(proxyService.Get(1), proxyService.Get(1));

            var proxyController = container.GetInstance<IController>();
            Assert.Equal(proxyService.Get(100), proxyController.Execute());
        }
    }
}
