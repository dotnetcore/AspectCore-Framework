using System;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class ProxyReflectorDelegationTests : DynamicProxyTestBase
    {
        [Fact]
        public void InterfaceProxy_WithRefParameterAndImplementation_HandlesByRef()
        {
            var impl = new RefParamServiceImpl();
            var proxy = ProxyGenerator.CreateInterfaceProxy<IRefParamService>(impl);
            Assert.NotNull(proxy);
            int value = 10;
            proxy.RefMethod(ref value);
            Assert.Equal(20, value);
        }

        [Fact]
        public void InterfaceProxy_WithOutParameterAndImplementation_HandlesByOut()
        {
            var impl = new RefParamServiceImpl();
            var proxy = ProxyGenerator.CreateInterfaceProxy<IRefParamService>(impl);
            Assert.NotNull(proxy);
            proxy.OutMethod(out int value);
            Assert.Equal(42, value);
        }

        [Fact]
        public void InterfaceProxy_WithMultipleRefParameters_HandlesAllByRef()
        {
            var impl = new RefParamServiceImpl();
            var proxy = ProxyGenerator.CreateInterfaceProxy<IRefParamService>(impl);
            Assert.NotNull(proxy);
            int a = 1, b = 2;
            proxy.MultipleRefMethod(ref a, ref b);
            Assert.Equal(2, a);
            Assert.Equal(4, b);
        }

        [Fact]
        public void InterfaceProxy_WithRefParameterAndReturnValue_HandlesBoth()
        {
            var impl = new RefParamServiceImpl();
            var proxy = ProxyGenerator.CreateInterfaceProxy<IRefParamService>(impl);
            Assert.NotNull(proxy);
            int value = 5;
            var result = proxy.RefMethodWithReturn(ref value);
            Assert.Equal(10, value);
            Assert.Equal(15, result);
        }

        public interface IRefParamService
        {
            void RefMethod(ref int value);
            void OutMethod(out int value);
            void MultipleRefMethod(ref int a, ref int b);
            int RefMethodWithReturn(ref int value);
        }

        public class RefParamServiceImpl : IRefParamService
        {
            public void RefMethod(ref int value) => value *= 2;
            public void OutMethod(out int value) => value = 42;
            public void MultipleRefMethod(ref int a, ref int b) { a *= 2; b *= 2; }
            public int RefMethodWithReturn(ref int value) { value *= 2; return value + 5; }
        }
    }
}
