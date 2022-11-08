using Autofac;
using System;
using AspectCore.Configuration;
using AspectCore.Extensions.Autofac;
using AspectCore.Extensions.Test.Fakes;
using Xunit;
using static AspectCoreTest.Autofac.Issues.PropertyInjectorProxyWithVirtualTests;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;

namespace AspectCoreTest.Autofac.Issues
{
    public class PropertiesAutowiredTests
    {
        public interface IService
        {
            int Run();
            string GetString(string key);
            string GetStringReturnValueModify(string key);
        }

        public interface IServiceTwo
        {
            int Run();
            string GetString(string key);
        }

        public class Service : IService
        {
            public IServiceTwo ServiceTwo { get; set; }

            [CacheInterceptor]
            public int Run()
            {
                return ServiceTwo.Run();
            }

            [StringParameterIntercept]
            public string GetString(string key)
            {
                return ServiceTwo.GetString(key);
            }

            [ReturnValueIntercept]
            public string GetStringReturnValueModify(string key)
            {
                return ServiceTwo.GetString(key);
            }
        }

        public class ServiceTwo : IServiceTwo
        {
            public int Run()
            {
                return 1024;
            }

            public string GetString(string key)
            {
                return key;
            }
        }

        public class ControllerAction
        {
            public IService Service { get; set; }

            public int ActionRun()
            {
                return Service.Run();
            }

            public string GetString(string key)
            {
                return Service.GetString(key);
            }

            public string GetStringReturnValueModify(string key)
            {
                return Service.GetStringReturnValueModify(key);
            }
        }

        public class StringParameterIntercept : AbstractInterceptorAttribute
        {
            public override Task Invoke(AspectContext context, AspectDelegate next)
            {
                context.Parameters[0] = "EMT";
                return context.Invoke(next);
            }
        }

        public class ReturnValueIntercept : AbstractInterceptorAttribute
        {
            public override async Task Invoke(AspectContext context, AspectDelegate next)
            {
                await next(context);
                context.ReturnValue = "Emilia";
            }
        }

        private ContainerBuilder CreateBuilder()
        {
            return new ContainerBuilder().RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate(next => ctx => next(ctx), Predicates.ForNameSpace("AspectCore.Extensions.Test.Issues"));
            });
        }

        /// <summary>
        /// 多层属性注入测试
        /// </summary>
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

        /// <summary>
        /// 多层属性注入下启用拦截器测试
        /// </summary>
        [Fact]
        public void PropertiesAutowiredWithMultilayerIntercept_Test()
        {
            var builder = CreateBuilder();
            builder.RegisterType<ControllerAction>().PropertiesAutowired();
            builder.RegisterType<Service>().AsImplementedInterfaces().PropertiesAutowired();
            builder.RegisterType<ServiceTwo>().AsImplementedInterfaces().PropertiesAutowired();
            var container = builder.Build();
            var action = container.Resolve<ControllerAction>();
            Assert.Equal("EMT", action.GetString("Lion"));
            Assert.Equal("Emilia", action.GetStringReturnValueModify("Lion"));
        }
    }
}