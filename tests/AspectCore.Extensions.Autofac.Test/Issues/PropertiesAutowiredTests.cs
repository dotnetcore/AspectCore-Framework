using Autofac;
using System;
using AspectCore.Configuration;
using AspectCore.Extensions.Autofac;
using AspectCore.Extensions.Test.Fakes;
using Xunit;
using static AspectCoreTest.Autofac.Issues.PropertyInjectorProxyWithVirtualTests;

namespace AspectCoreTest.Autofac.Issues
{
    public class PropertiesAutowiredTests
    {
        public interface IService
        {
            int Run();
        }

        public interface IServiceTwo
        {
            int Run();
        }

        public class Service : IService
        {
            public IServiceTwo ServiceTwo { get; set; }

            [CacheInterceptor]
            public int Run()
            {
                return ServiceTwo.Run();
            }
        }

        public class ServiceTwo : IServiceTwo
        {
            public int Run()
            {
                return 1024;
            }
        }

        public class ControllerAction
        {
            public IService Service { get; set; }

            public int ActionRun()
            {
                return Service.Run();
            }
        }

        private ContainerBuilder CreateBuilder()
        {
            return new ContainerBuilder().RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate(next => ctx => next(ctx), Predicates.ForNameSpace("AspectCore.Extensions.Test.Issues"));
            });
        }

        [Fact]
        public void PropertiesAutowiredWithMultilayer_Test()
        {
            var builder = CreateBuilder();
            builder.RegisterType<ControllerAction>().PropertiesAutowired();
            builder.RegisterType<Service>().AsImplementedInterfaces().PropertiesAutowired();
            builder.RegisterType<ServiceTwo>().AsImplementedInterfaces().PropertiesAutowired();
            var container = builder.Build();
            var action = container.Resolve<ControllerAction>();
            action.ActionRun();
        }
    }
}