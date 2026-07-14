using System;
using System.Collections.Generic;
using System.Linq;
using AspectCore.DynamicProxy.ProxyBuilder.Builders;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class GenericParameterNodeFactoryTests
    {
        [Fact]
        public void FromType_WithNonGenericType_ReturnsEmpty()
        {
            var result = GenericParameterNodeFactory.FromType(typeof(string));
            Assert.Empty(result);
        }

        [Fact]
        public void FromType_WithNoConstraints_ReturnsParametersWithNoConstraints()
        {
            var result = GenericParameterNodeFactory.FromType(typeof(NoConstraint<>));
            Assert.Single(result);
            Assert.Equal("T", result[0].Name);
        }

        [Fact]
        public void FromType_WithStructConstraint_ReturnsParametersWithStructConstraint()
        {
            var result = GenericParameterNodeFactory.FromType(typeof(StructConstraint<>));
            Assert.Single(result);
            Assert.Equal("T", result[0].Name);
        }

        [Fact]
        public void FromType_WithClassConstraint_ReturnsParametersWithClassConstraint()
        {
            var result = GenericParameterNodeFactory.FromType(typeof(ClassConstraint<>));
            Assert.Single(result);
            Assert.Equal("T", result[0].Name);
        }

        [Fact]
        public void FromType_WithNewConstraint_ReturnsParametersWithNewConstraint()
        {
            var result = GenericParameterNodeFactory.FromType(typeof(NewConstraint<>));
            Assert.Single(result);
            Assert.Equal("T", result[0].Name);
        }

        [Fact]
        public void FromType_WithClassAndNewConstraint_ReturnsParametersWithBothConstraints()
        {
            var result = GenericParameterNodeFactory.FromType(typeof(ClassAndNewConstraint<>));
            Assert.Single(result);
            Assert.Equal("T", result[0].Name);
        }

        [Fact]
        public void FromType_WithMultipleConstraints_ReturnsParametersWithAllConstraints()
        {
            var result = GenericParameterNodeFactory.FromType(typeof(MultipleConstraints<,>));
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void FromType_WithInterfaceConstraint_ReturnsParametersWithInterfaceConstraint()
        {
            var result = GenericParameterNodeFactory.FromType(typeof(InterfaceConstraint<>));
            Assert.Single(result);
            Assert.Equal("T", result[0].Name);
        }

        [Fact]
        public void FromType_WithUnmanagedConstraint_ReturnsParametersWithUnmanagedConstraint()
        {
            var result = GenericParameterNodeFactory.FromType(typeof(UnmanagedConstraint<>));
            Assert.Single(result);
            Assert.Equal("T", result[0].Name);
        }

        [Fact]
        public void FromType_WithBaseTypeConstraint_ReturnsParametersWithBaseTypeConstraint()
        {
            var result = GenericParameterNodeFactory.FromType(typeof(BaseTypeConstraint<>));
            Assert.Single(result);
            Assert.Equal("T", result[0].Name);
        }

        [Fact]
        public void FromMethod_WithNonGenericMethod_ReturnsEmpty()
        {
            var method = typeof(TestService).GetMethod(nameof(TestService.NonGenericMethod));
            var result = GenericParameterNodeFactory.FromMethod(method);
            Assert.Empty(result);
        }

        [Fact]
        public void FromMethod_WithGenericMethod_ReturnsParameters()
        {
            var method = typeof(TestService).GetMethod(nameof(TestService.GenericMethod));
            var result = GenericParameterNodeFactory.FromMethod(method);
            Assert.Single(result);
        }

        public class NoConstraint<T> { }
        public class StructConstraint<T> where T : struct { }
        public class ClassConstraint<T> where T : class { }
        public class NewConstraint<T> where T : new() { }
        public class ClassAndNewConstraint<T> where T : class, new() { }
        public class MultipleConstraints<T1, T2> where T1 : class where T2 : struct { }
        public class InterfaceConstraint<T> where T : IComparable<T> { }
        public class UnmanagedConstraint<T> where T : unmanaged { }
        public class BaseTypeConstraint<T> where T : TestService { }

        public class TestService
        {
            public void NonGenericMethod() { }
            public T GenericMethod<T>(T value) => value;
        }
    }
}
