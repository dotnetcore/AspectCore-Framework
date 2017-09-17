using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;
using F_IService = AspectCore.Tests.IService;

namespace AspectCore.Tests.DynamicProxy
{
    public class ProxyGeneratorTest : DynamicProxyTestBase
    {
        [Fact]
        public void CreateInterfaceProxy_Without_ImplType()
        {
            var serviceProxy = ProxyGenerator.CreateInterfaceProxy<F_IService>();
            Assert.NotNull(serviceProxy);
            Assert.IsAssignableFrom<F_IService>(serviceProxy);
            Assert.Equal(default(Guid), serviceProxy.Id);
            var id = Guid.NewGuid();
            serviceProxy.Id = id;
            Assert.Equal(id, serviceProxy.Id);
            Assert.Equal(default(ILogger), serviceProxy.Logger);

            var serviceProxy2 = ProxyGenerator.CreateInterfaceProxy<IService>();
            Assert.NotNull(serviceProxy2);
            Assert.IsAssignableFrom<IService>(serviceProxy2);
        }

        [Fact]
        public void CreateInterfaceProxy_With_ImplType()
        {
            var id = Guid.NewGuid();
            var serviceProxy = ProxyGenerator.CreateInterfaceProxy<F_IService, Service>(args: new object[] { id });
            Assert.NotNull(serviceProxy);
            Assert.IsAssignableFrom<F_IService>(serviceProxy);
            Assert.Equal(id, serviceProxy.Id);
            Assert.Equal(default(ILogger), serviceProxy.Logger);
            var logger = new Logger();
            serviceProxy.Logger = logger;
            Assert.Equal(logger, serviceProxy.Logger);
        }

        [Fact]
        public void CreateClassProxy()
        {
            var serviceProxy = ProxyGenerator.CreateClassProxy<BaseService>();
            Assert.IsNotType<BaseService>(serviceProxy);
            var name = serviceProxy.GetServiceName();
            Assert.Equal("CreateClassProxy", name);
        }

        protected override void Configure(IAspectConfiguration configuration)
        {
            configuration.Interceptors.AddDelegate((ctx, next) => next(ctx), Predicates.ForService("IService"));
            configuration.Interceptors.AddDelegate( async (ctx, next) =>
            {
                await next(ctx);
                ctx.ReturnValue = "CreateClassProxy";
            }
            , Predicates.ForService("*BaseService"));
        }

        public class Service : F_IService
        {
            public Guid Id { get; set; }
            public ILogger Logger { get; set; }

            public Service(Guid id)
            {
                Id = id;
            }
        }

        public class BaseService
        {
            public virtual string GetServiceName()
            {
                return "BaseService";
            }
        }
    }

    public interface IService { }
}
