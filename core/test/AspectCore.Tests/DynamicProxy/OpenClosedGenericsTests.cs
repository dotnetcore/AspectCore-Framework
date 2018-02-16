using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.Configuration;
using Xunit;
using AspectCore.DynamicProxy;
using System.Reflection;
using AspectCore.DynamicProxy.Parameters;

namespace AspectCore.Tests.DynamicProxy
{
    public class OpenClosedGenericsTests : DynamicProxyTestBase
    {
        [Fact]
        public void CreateInterfaceProxy_Without_ImplType()
        {
            var serviceProxy = ProxyGenerator.CreateInterfaceProxy<IFakeOpenGenericService<PocoClass>>();
            Assert.NotNull(serviceProxy);
            Assert.IsAssignableFrom<IFakeOpenGenericService<PocoClass>>(serviceProxy);
        }

        [Fact]
        public void CreateInterfaceProxyType_Without_ImplType()
        {
            var configuration = new AspectConfiguration();
            configuration.Interceptors.AddTyped<EnableParameterAspectInterceptor>();
            var validatorBuilder = new AspectValidatorBuilder(configuration);
            var proxyTypeGenerator = new ProxyTypeGenerator(validatorBuilder);
            var proxyType = proxyTypeGenerator.CreateInterfaceProxyType(typeof(IFakeOpenGenericService<>));
            var instance = Activator.CreateInstance(proxyType.MakeGenericType(typeof(PocoClass)), new object[] { null });
            var field = instance.GetType().GetTypeInfo().GetField("_implementation", BindingFlags.Instance | BindingFlags.NonPublic);
            var targetInstance = field.GetValue(instance);
            Assert.NotEqual(instance, targetInstance);
            Assert.NotEqual(instance.GetType(), targetInstance.GetType());
        }

        [Fact]
        public void CreateInterfaceProxy_With_ImplType()
        {
            var serviceProxy = ProxyGenerator.CreateInterfaceProxy<IFakeOpenGenericService<PocoClass>, FakeOpenGenericService<PocoClass>>(new PocoClass());
            Assert.NotNull(serviceProxy);
            Assert.IsAssignableFrom<IFakeOpenGenericService<PocoClass>>(serviceProxy);
        }

        [Fact]
        public void CreateInterfaceProxyType_Wit_ImplType()
        {
            var configuration = new AspectConfiguration();
            configuration.Interceptors.AddTyped<EnableParameterAspectInterceptor>();
            var validatorBuilder = new AspectValidatorBuilder(configuration);
            var proxyTypeGenerator = new ProxyTypeGenerator(validatorBuilder);
            var proxyType = proxyTypeGenerator.CreateInterfaceProxyType(typeof(IFakeOpenGenericService<>), typeof(FakeOpenGenericService<>));
            var instance = Activator.CreateInstance(proxyType.MakeGenericType(typeof(PocoClass)), new object[] { null, new FakeOpenGenericService<PocoClass>(null) });
            var field = instance.GetType().GetTypeInfo().GetField("_implementation", BindingFlags.Instance | BindingFlags.NonPublic);
            var targetInstance = field.GetValue(instance);
            Assert.NotEqual(instance, targetInstance);
            Assert.NotEqual(instance.GetType(), targetInstance.GetType());
        }

        protected override void Configure(IAspectConfiguration configuration)
        {
            configuration.Interceptors.AddTyped<EnableParameterAspectInterceptor>();
        }
    }
}