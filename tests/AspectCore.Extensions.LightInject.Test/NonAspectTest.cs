using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.LightInject;
using LightInject;
using Xunit;

namespace AspectCoreTest.LightInject
{
    public class NonAspectTest
    {
        [Fact]
        public void Aspect_Test()
        {
            var proxy = GetService();
            Assert.Equal("lemon", proxy.Aspect());
        }

        [Fact]
        public void NonAspect_Test()
        {
            var proxy = GetService();
            Assert.Equal("le", proxy.NonAspect());
        }

        [Fact]
        public void MyNonAspect_Test()
        {
            var proxy = GetService();
            Assert.Equal("le", proxy.MyNonAspect());
        }

        private IFakeNonAspect GetService()
        {
            var builder = new ServiceContainer();
            builder.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    await ctx.Invoke(next);
                    ctx.ReturnValue = "lemon";
                });
            });
            builder.Register<IFakeNonAspect, FakeNonAspect>();
            return builder.GetInstance<IFakeNonAspect>();
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
