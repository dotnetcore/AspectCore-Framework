using System;
using System.Collections.Generic;
using AspectCore.Extensions;
using Xunit;

namespace AspectCore.Core.Tests.Extensions
{
    public class TypeExtensionsCovariantTests
    {
        [Fact]
        public void IsCovariantReturnAssignableFrom_WithByRefAndNonByRef_ReturnsFalse()
        {
            var byRefType = typeof(int).MakeByRefType();
            var result = byRefType.IsCovariantReturnAssignableFrom(typeof(int));
            Assert.False(result);
        }

        [Fact]
        public void IsCovariantReturnAssignableFrom_WithNonByRefAndByRef_ReturnsFalse()
        {
            var byRefType = typeof(int).MakeByRefType();
            var result = typeof(int).IsCovariantReturnAssignableFrom(byRefType);
            Assert.False(result);
        }

        [Fact]
        public void IsCovariantReturnAssignableFrom_WithBothByRef_ComparesElementTypes()
        {
            var byRefType1 = typeof(int).MakeByRefType();
            var byRefType2 = typeof(int).MakeByRefType();
            var result = byRefType1.IsCovariantReturnAssignableFrom(byRefType2);
            Assert.True(result);
        }

        [Fact]
        public void IsCovariantReturnEquivalentTo_WithByRefAndNonByRef_ReturnsFalse()
        {
            var byRefType = typeof(int).MakeByRefType();
            var result = byRefType.IsCovariantReturnEquivalentTo(typeof(int));
            Assert.False(result);
        }

        [Fact]
        public void IsCovariantReturnEquivalentTo_WithBothByRef_ReturnsTrue()
        {
            var byRefType1 = typeof(int).MakeByRefType();
            var byRefType2 = typeof(int).MakeByRefType();
            var result = byRefType1.IsCovariantReturnEquivalentTo(byRefType2);
            Assert.True(result);
        }

        [Fact]
        public void IsCovariantReturnEquivalentTo_WithDifferentGenericParameters_ReturnsFalse()
        {
            // Generic parameters with different positions are not equivalent
            var type1 = typeof(Func<int>).GetGenericArguments()[0];
            var type2 = typeof(Func<string>).GetGenericArguments()[0];
            var result = type1.IsCovariantReturnEquivalentTo(type2);
            Assert.False(result);
        }

        [Fact]
        public void IsCovariantReturnAssignableFrom_WithCovariantGenericParameter_ReturnsTrue()
        {
            // Test covariant assignment: IEnumerable<Derived> is assignable to IEnumerable<Base>
            var result = typeof(IEnumerable<Base>).IsCovariantReturnAssignableFrom(typeof(IEnumerable<Derived>));
            Assert.True(result);
        }

        [Fact]
        public void IsCovariantReturnAssignableFrom_WithNonCovariantGenericParameter_ReturnsFalse()
        {
            // Test non-covariant assignment: IComparer<Derived> is NOT assignable to IComparer<Base>
            var result = typeof(IComparer<Base>).IsCovariantReturnAssignableFrom(typeof(IComparer<Derived>));
            Assert.False(result);
        }

        [Fact]
        public void IsCovariantReturnAssignableFrom_WithArrayType_ReturnsTrue()
        {
            // Array covariance: Derived[] is assignable to Base[]
            var result = typeof(Base[]).IsCovariantReturnAssignableFrom(typeof(Derived[]));
            Assert.True(result);
        }

        [Fact]
        public void IsCovariantReturnAssignableFrom_WithDifferentArrayRanks_ReturnsFalse()
        {
            var result = typeof(Base[]).IsCovariantReturnAssignableFrom(typeof(Base[,]));
            Assert.False(result);
        }

        [Fact]
        public void IsCovariantReturnAssignableFrom_WithGenericTypeDefinition_ReturnsTrue()
        {
            var result = typeof(IEnumerable<>).IsCovariantReturnAssignableFrom(typeof(IEnumerable<>));
            Assert.True(result);
        }

        [Fact]
        public void IsAssignableFromGenericTypeDefinition_WithNonGenericTypeDefinition_ReturnsFalse()
        {
            var result = typeof(int).IsAssignableFromGenericTypeDefinition(typeof(int));
            Assert.False(result);
        }

        [Fact]
        public void IsAssignableFromGenericTypeDefinition_WithMatchingGenericDefinition_ReturnsTrue()
        {
            var result = typeof(IEnumerable<>).IsAssignableFromGenericTypeDefinition(typeof(IEnumerable<>));
            Assert.True(result);
        }

        [Fact]
        public void IsGenericParameterCovariant_WithNonGenericParameter_ReturnsFalse()
        {
            var result = typeof(int).IsGenericParameterCovariant();
            Assert.False(result);
        }

        [Fact]
        public void IsGenericParameterCovariant_WithCovariantParameter_ReturnsTrue()
        {
            // IEnumerable<out T> - T is covariant
            var type = typeof(IEnumerable<>).GetGenericArguments()[0];
            var result = type.IsGenericParameterCovariant();
            Assert.True(result);
        }

        [Fact]
        public void GetInheritanceDepth_WithNull_ReturnsZero()
        {
            Type type = null;
            var result = type.GetInheritanceDepth();
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetInheritanceDepth_WithDirectInheritance_ReturnsCorrectDepth()
        {
            var result = typeof(Derived).GetInheritanceDepth();
            Assert.Equal(1, result);
        }

        [Fact]
        public void GetInheritanceDepth_WithObject_ReturnsMinusOne()
        {
            var result = typeof(object).GetInheritanceDepth();
            Assert.Equal(-1, result);
        }

        [Fact]
        public void GetCovariantReturnMethods_WithTypeWithoutCovariantReturns_ReturnsEmpty()
        {
            var result = typeof(Base).GetCovariantReturnMethods();
            Assert.Empty(result);
        }

        [Fact]
        public void IsOverriddenByCovariantReturnMethod_WithNonCovariantMethod_ReturnsFalse()
        {
            var baseMethod = typeof(Base).GetMethod("GetValue");
            var derivedMethod = typeof(Derived).GetMethod("GetValue");
            var result = baseMethod.IsOverriddenByCovariantReturnMethod(derivedMethod);
            Assert.False(result);
        }

        public class Base
        {
            public virtual object GetValue() => null;
        }

        public class Derived : Base
        {
            public override object GetValue() => "derived";
        }

        public class Base2 { }
        public class Derived2 : Base2 { }
    }
}
