using System;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class ProxyGenericConstraintTests : DynamicProxyTestBase
    {
        [Fact]
        public void ClassProxy_GenericWithBaseTypeConstraint_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<GenericWithBaseConstraint<TestService>>();
            Assert.NotNull(proxy);
        }

        [Fact]
        public void ClassProxy_GenericWithInterfaceConstraint_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<GenericWithInterfaceConstraint<string>>();
            Assert.NotNull(proxy);
        }

        [Fact]
        public void ClassProxy_GenericWithBaseAndInterfaceConstraint_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<GenericWithBaseAndInterfaceConstraint<ComparableTestService>>();
            Assert.NotNull(proxy);
        }

        [Fact]
        public void InterfaceProxy_GenericWithBaseTypeConstraint_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateInterfaceProxy<IGenericWithBaseConstraint<TestService>>();
            Assert.NotNull(proxy);
        }

        [Fact]
        public void InterfaceProxy_GenericWithInterfaceConstraint_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateInterfaceProxy<IGenericWithInterfaceConstraint<string>>();
            Assert.NotNull(proxy);
        }

        [Fact]
        public void ClassProxy_GenericMethodWithConstraints_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ServiceWithGenericMethodConstraints>();
            Assert.NotNull(proxy);
        }

        public class TestService { }

        public class ComparableTestService : TestService, IComparable<ComparableTestService>
        {
            public int CompareTo(ComparableTestService other) => 0;
        }

        public class GenericWithBaseConstraint<T> where T : TestService
        {
            public virtual T GetValue() => default;
        }

        public class GenericWithInterfaceConstraint<T> where T : IComparable<T>
        {
            public virtual T GetValue() => default;
        }

        public class GenericWithBaseAndInterfaceConstraint<T> where T : TestService, IComparable<T>, new()
        {
            public virtual T GetValue() => default;
        }

        public interface IGenericWithBaseConstraint<T> where T : TestService
        {
            T GetValue();
        }

        public interface IGenericWithInterfaceConstraint<T> where T : IComparable<T>
        {
            T GetValue();
        }

        public class ServiceWithGenericMethodConstraints
        {
            public virtual T GetValue<T>() where T : TestService, new() => default;
        }
    }
}
