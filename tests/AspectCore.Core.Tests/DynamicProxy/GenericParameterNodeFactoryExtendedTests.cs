using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.DynamicProxy.ProxyBuilder.Builders;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class GenericParameterNodeFactoryExtendedTests
    {
        [Fact]
        public void FromType_WithNotNullConstraint_ReturnsParametersWithSpecialConstraintMask()
        {
            // Tests line 66: SpecialConstraintMask return path
            // `notnull` constraint sets SpecialConstraintMask in the runtime
            var result = GenericParameterNodeFactory.FromType(typeof(NotNullConstraint<>));
            Assert.Single(result);
            Assert.Equal("T", result[0].Name);
        }

        [Fact]
        public void FromType_WithCovariantInterface_ReturnsParameters()
        {
            // Tests line 75: default return None (when only variance flags are set)
            // Covariant interface type parameter has Covariant flag which doesn't match any constraint check
            var result = GenericParameterNodeFactory.FromType(typeof(ICovariant<>));
            Assert.Single(result);
            Assert.Equal("T", result[0].Name);
        }

        [Fact]
        public void FromType_WithContravariantInterface_ReturnsParameters()
        {
            // Also tests line 75 with contravariant
            var result = GenericParameterNodeFactory.FromType(typeof(IContravariant<>));
            Assert.Single(result);
            Assert.Equal("T", result[0].Name);
        }

        [Fact]
        public void FromType_WithMultipleConstraintsAndVariance_ReturnsParameters()
        {
            // Tests with both variance and constraints
            var result = GenericParameterNodeFactory.FromType(typeof(ICovariantWithConstraint<,>));
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void FromMethod_WithGenericMethodHavingConstraints_ReturnsParameters()
        {
            // Tests FromMethod with a generic method that has constraints
            var method = typeof(GenericMethodWithConstraint).GetMethod(nameof(GenericMethodWithConstraint.Process));
            var result = GenericParameterNodeFactory.FromMethod(method);
            Assert.Single(result);
        }

        [Fact]
        public void FromMethod_WithGenericMethodNoConstraints_ReturnsParameters()
        {
            // Tests FromMethod with a simple generic method
            var method = typeof(SimpleGenericMethod).GetMethod(nameof(SimpleGenericMethod.Echo));
            var result = GenericParameterNodeFactory.FromMethod(method);
            Assert.Single(result);
        }

        [Fact]
        public void FromType_WithClassAndNewAndVariance_ReturnsParameters()
        {
            // Tests the ReferenceTypeConstraint | DefaultConstructorConstraint path (line 69)
            // combined with variance on an interface
            var result = GenericParameterNodeFactory.FromType(typeof(IClassNewConstraint<>));
            Assert.Single(result);
            Assert.Equal("T", result[0].Name);
        }

        [Fact]
        public void FromType_WithAllConstraintTypes_ReturnsAllParameters()
        {
            // Tests a type with multiple generic parameters covering different constraint types
            var result = GenericParameterNodeFactory.FromType(typeof(MultiConstraintType<,,,>));
            Assert.Equal(4, result.Count);
        }

        [Fact]
        public void FromType_WithBaseTypeConstraint_VerifiesBaseTypeIsSet()
        {
            // Verify that base type constraint is properly captured
            var result = GenericParameterNodeFactory.FromType(typeof(BaseTypeConstraint2<>));
            Assert.Single(result);
            Assert.NotNull(result[0].BaseTypeConstraint);
        }

        [Fact]
        public void FromType_WithInterfaceConstraint_VerifiesInterfaceConstraintsAreSet()
        {
            // Verify that interface constraints are properly captured
            var result = GenericParameterNodeFactory.FromType(typeof(InterfaceConstraint2<>));
            Assert.Single(result);
            Assert.NotEmpty(result[0].InterfaceConstraints);
        }

        #region Test Types

        // notnull constraint - maps to SpecialConstraintMask in the runtime
        public class NotNullConstraint<T> where T : notnull { }

        // Covariant interface - only Covariant flag set, no constraints
        public interface ICovariant<out T> { }

        // Contravariant interface - only Contravariant flag set, no constraints
        public interface IContravariant<in T> { }

        // Interface with both variance and constraints
        public interface ICovariantWithConstraint<out T, in T2> where T : class { }

        // Generic method with constraints
        public static class GenericMethodWithConstraint
        {
            public static void Process<T>(T value) where T : struct { }
        }

        // Simple generic method
        public static class SimpleGenericMethod
        {
            public static T Echo<T>(T value) => value;
        }

        // Interface with class + new constraint
        public interface IClassNewConstraint<T> where T : class, new() { }

        // Type with multiple constraint types
        public class MultiConstraintType<T1, T2, T3, T4>
            where T1 : struct
            where T2 : class
            where T3 : new()
            where T4 : notnull
        { }

        // Base type constraint
        public class BaseTypeConstraint2<T> where T : BaseConstraintClass { }

        public class BaseConstraintClass { }

        // Interface constraint
        public class InterfaceConstraint2<T> where T : IComparable<T> { }

        #endregion
    }
}
