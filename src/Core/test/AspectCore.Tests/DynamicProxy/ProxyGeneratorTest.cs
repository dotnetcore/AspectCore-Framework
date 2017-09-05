using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Tests.DynamicProxy
{
    public class ProxyGeneratorTest : DynamicProxyTestBase
    {
        [Fact]
        public void CreateInterfaceProxy_Without_ImplType()
        {
            var serviceProxy = ProxyGenerator.CreateInterfaceProxy<IService>();
            Assert.NotNull(serviceProxy);
            Assert.IsAssignableFrom<IService>(serviceProxy);
            Assert.Equal(default(Guid), serviceProxy.Id);
            var id = Guid.NewGuid();
            serviceProxy.Id = id;
            Assert.Equal(id, serviceProxy.Id);
            Assert.Equal(default(ILogger), serviceProxy.Logger);
        }

        [Fact]
        public void CreateInterfaceProxy_With_ImplType()
        {
            var id = Guid.NewGuid();
            var serviceProxy = ProxyGenerator.CreateInterfaceProxy<IService, Service>(args: new object[] { id });
            Assert.NotNull(serviceProxy);
            Assert.IsAssignableFrom<IService>(serviceProxy);
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

        public class Service : IService
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
}
