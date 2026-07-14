using System;
using System.Collections.Generic;
using System.Linq;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class GenericParameterProxyTests : DynamicProxyTestBase
    {
        [Fact]
        public void ClassProxy_GenericWithClassConstraint_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<GenericWithClassConstraint<TestService>>();
            Assert.NotNull(proxy);
            Assert.IsAssignableFrom<GenericWithClassConstraint<TestService>>(proxy);
        }

        [Fact]
        public void ClassProxy_GenericWithNewConstraint_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<GenericWithNewConstraint<TestService>>();
            Assert.NotNull(proxy);
        }

        [Fact]
        public void ClassProxy_GenericWithStructConstraint_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<GenericWithStructConstraint<int>>();
            Assert.NotNull(proxy);
        }

        [Fact]
        public void InterfaceProxy_GenericWithConstraints_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateInterfaceProxy<IGenericWithConstraint<int>>();
            Assert.NotNull(proxy);
        }

        [Fact]
        public void ClassProxy_GenericWithMultipleConstraints_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<GenericWithMultipleConstraints<TestService>>();
            Assert.NotNull(proxy);
        }

        [Fact]
        public void ClassProxy_GenericMethod_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ServiceWithGenericMethod>();
            Assert.NotNull(proxy);
        }

        public class TestService { }

        public class GenericWithClassConstraint<T> where T : class
        {
            public virtual T GetValue() => default;
        }

        public class GenericWithNewConstraint<T> where T : new()
        {
            public virtual T Create() => new T();
        }

        public class GenericWithStructConstraint<T> where T : struct
        {
            public virtual T GetValue() => default;
        }

        public interface IGenericWithConstraint<T> where T : struct
        {
            T GetValue();
        }

        public class GenericWithMultipleConstraints<T> where T : class, new()
        {
            public virtual T Create() => new T();
        }

        public class ServiceWithGenericMethod
        {
            public virtual T Echo<T>(T value) => value;
            public virtual List<T> CreateList<T>() => new List<T>();
        }
    }
}
