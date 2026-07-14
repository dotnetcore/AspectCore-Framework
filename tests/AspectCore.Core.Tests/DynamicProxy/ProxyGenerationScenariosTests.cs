using System;
using System.Collections.Generic;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.Parameters;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class ProxyGenerationScenariosTests : DynamicProxyTestBase
    {
        [Fact]
        public void ClassProxy_WithConstructorArguments_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy(typeof(ServiceWithCtor), typeof(ServiceWithCtor), new object[] { "test" });
            Assert.NotNull(proxy);
            Assert.IsAssignableFrom<ServiceWithCtor>(proxy);
        }

        [Fact]
        public void ClassProxy_WithDefaultParameterValues_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ServiceWithDefaults>();
            Assert.NotNull(proxy);
            var result = proxy.MethodWithDefaults();
            Assert.Equal("default", result);
        }

        [Fact]
        public void ClassProxy_WithMultipleConstructors_GeneratesProxy()
        {
            var proxy1 = ProxyGenerator.CreateClassProxy(typeof(MultiCtorService), typeof(MultiCtorService), new object[] { "test" });
            Assert.NotNull(proxy1);
            var proxy2 = ProxyGenerator.CreateClassProxy(typeof(MultiCtorService), typeof(MultiCtorService), new object[] { 42 });
            Assert.NotNull(proxy2);
        }

        [Fact]
        public void InterfaceProxy_WithGenericMethod_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateInterfaceProxy<IGenericMethodService>();
            Assert.NotNull(proxy);
            // Without an implementation, returns default value
            var result = proxy.Echo<int>(42);
            Assert.Equal(0, result);
        }

        [Fact]
        public void InterfaceProxy_WithOutParameter_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateInterfaceProxy<IOutParamService>();
            Assert.NotNull(proxy);
            proxy.MethodWithOut(out int value);
            Assert.Equal(0, value);
        }

        [Fact]
        public void InterfaceProxy_WithRefParameter_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateInterfaceProxy<IRefParamService>();
            Assert.NotNull(proxy);
            int value = 42;
            proxy.MethodWithRef(ref value);
        }

        [Fact]
        public void ClassProxy_WithGenericTypeConstraints_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ConstrainedGenericService<int>>();
            Assert.NotNull(proxy);
        }

        [Fact]
        public void InterfaceProxy_WithGenericTypeConstraints_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateInterfaceProxy<IConstrainedGenericService<int>>();
            Assert.NotNull(proxy);
        }

        [Fact]
        public void ClassProxy_WithNullableParameter_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ServiceWithNullable>();
            Assert.NotNull(proxy);
            var result = proxy.MethodWithNullable(null);
            Assert.Null(result);
        }

        [Fact]
        public void InterfaceProxy_WithMultipleInterfaces_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateInterfaceProxy<IMultiInterfaceService>();
            Assert.NotNull(proxy);
            Assert.IsAssignableFrom<IMultiInterfaceService>(proxy);
        }

        public class ServiceWithCtor
        {
            public string Name { get; }
            public ServiceWithCtor(string name) => Name = name;
            public virtual string GetName() => Name;
        }

        public class ServiceWithDefaults
        {
            public virtual string MethodWithDefaults(string value = "default") => value;
        }

        public class MultiCtorService
        {
            public string StringValue { get; }
            public int IntValue { get; }
            public MultiCtorService(string value) => StringValue = value;
            public MultiCtorService(int value) => IntValue = value;
        }

        public interface IGenericMethodService
        {
            T Echo<T>(T value);
        }

        public interface IOutParamService
        {
            void MethodWithOut(out int value);
        }

        public interface IRefParamService
        {
            void MethodWithRef(ref int value);
        }

        public interface IConstrainedGenericService<T> where T : struct
        {
            T GetValue();
        }

        public class ConstrainedGenericService<T> : IConstrainedGenericService<T> where T : struct
        {
            public virtual T GetValue() => default;
        }

        public class ServiceWithNullable
        {
            public virtual string MethodWithNullable(string value) => value;
        }

        public interface IMultiInterfaceService : IDisposable
        {
            void DoSomething();
        }
    }
}
