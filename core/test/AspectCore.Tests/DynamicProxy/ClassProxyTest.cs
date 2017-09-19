using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Tests.DynamicProxy
{
    public class ClassProxyTest : DynamicProxyTestBase
    {
        [Fact]
        public void NonVirtualMethod_Test()
        {
            var proxy = ProxyGenerator.CreateClassProxy<FakeClass>();
            Assert.True(proxy.IsProxy());
            proxy.Name = "123";
            Assert.Equal(proxy.Name, proxy.Msg);
            Assert.Equal(proxy.Name, proxy.Context);
            Assert.Equal(proxy.Msg, proxy.Context);
        }

        protected override void Configure(IAspectConfiguration configuration)
        {
            configuration.Interceptors.AddDelegate((ctx, next) =>
            {
                return next(ctx);
            });
            configuration.NonAspectPredicates.Add(m => m.DeclaringType == typeof(IFakeClass2));
        }

        public interface IFakeClass
        {
            string Msg { get; }
        }

        public interface IFakeClass2
        {
            string Context { get; }
        }

        public class FakeClass : IFakeClass, IFakeClass2
        {
            public string Name { get; set; }

            public string Msg => Name;

            public string Context => Name;
        }
    }
}