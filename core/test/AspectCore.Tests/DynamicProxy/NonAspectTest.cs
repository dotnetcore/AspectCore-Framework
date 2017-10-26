using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Tests.DynamicProxy
{
    public class NonAspectTest:DynamicProxyTestBase
    {
        [Fact]
        public void Aspect_Test()
        {
            var proxy = ProxyGenerator.CreateInterfaceProxy<IFakeNonAspect, FakeNonAspect>();
            Assert.Equal("lemon", proxy.Aspect());
        }

        [Fact]
        public void NonAspect_Test()
        {
            var proxy = ProxyGenerator.CreateInterfaceProxy<IFakeNonAspect, FakeNonAspect>();
            Assert.Equal("le", proxy.NonAspect());
        }

        [Fact]
        public void MyNonAspect_Test()
        {
            var proxy = ProxyGenerator.CreateInterfaceProxy<IFakeNonAspect, FakeNonAspect>();
            Assert.Equal("le", proxy.MyNonAspect());
        }

        protected override void Configure(IAspectConfiguration configuration)
        {
            base.Configure(configuration);
            configuration.Interceptors.AddDelegate(async (ctx, next) =>
            {
                await ctx.Invoke(next);
                ctx.ReturnValue = "lemon";
            });
        }
    }

    public interface IFakeNonAspect
    {
        string Aspect();

        [NonAspect]
        string NonAspect();

        [MyNonAspect]
        string MyNonAspect();
    }

    public class FakeNonAspect : IFakeNonAspect
    {
        public string Aspect()
        {
            return "le";
        }

        public string MyNonAspect()
        {
            return "le";
        }

        public string NonAspect()
        {
            return "le";
        }
    }

    public class MyNonAspect : NonAspectAttribute
    {
    }
}
