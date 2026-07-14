using System;
using System.Collections.Generic;
using AspectCore.Extensions;
using Xunit;

namespace AspectCore.Core.Tests.Extensions
{
    public class TypeExtensionsExtendedTests
    {
        [Fact]
        public void IsCovariantReturnEquivalentTo_WithDifferentGenericParameterPositions_ReturnsFalse()
        {
            var type1 = typeof(Func<,>).GetGenericArguments()[0];
            var type2 = typeof(Func<,>).GetGenericArguments()[1];
            var result = type1.IsCovariantReturnEquivalentTo(type2);
            Assert.False(result);
        }

        [Fact]
        public void IsCovariantReturnEquivalentTo_WithSameGenericParameter_ReturnsTrue()
        {
            var type1 = typeof(Func<>).GetGenericArguments()[0];
            var type2 = typeof(Func<>).GetGenericArguments()[0];
            var result = type1.IsCovariantReturnEquivalentTo(type2);
            Assert.True(result);
        }

        [Fact]
        public void IsCovariantReturnEquivalentTo_WithArrays_ReturnsTrue()
        {
            var result = typeof(int[]).IsCovariantReturnEquivalentTo(typeof(int[]));
            Assert.True(result);
        }

        [Fact]
        public void IsCovariantReturnEquivalentTo_WithDifferentArrayRanks_ReturnsFalse()
        {
            var result = typeof(int[]).IsCovariantReturnEquivalentTo(typeof(int[,]));
            Assert.False(result);
        }

        [Fact]
        public void IsCovariantReturnEquivalentTo_WithSameGenericType_ReturnsTrue()
        {
            var result = typeof(IEnumerable<int>).IsCovariantReturnEquivalentTo(typeof(IEnumerable<int>));
            Assert.True(result);
        }

        [Fact]
        public void IsCovariantReturnEquivalentTo_WithDifferentGenericTypeDefinitions_ReturnsFalse()
        {
            var result = typeof(IEnumerable<int>).IsCovariantReturnEquivalentTo(typeof(IList<int>));
            Assert.False(result);
        }

        [Fact]
        public void IsCovariantReturnEquivalentTo_WithNonMatchingTypes_ReturnsFalse()
        {
            var result = typeof(int).IsCovariantReturnEquivalentTo(typeof(string));
            Assert.False(result);
        }

        [Fact]
        public void IsCovariantReturnAssignableFrom_WithArrayCovariance_ReturnsTrue()
        {
            var result = typeof(object[]).IsCovariantReturnAssignableFrom(typeof(string[]));
            Assert.True(result);
        }

        [Fact]
        public void IsCovariantReturnAssignableFrom_WithDifferentArrayRanks_ReturnsFalse()
        {
            var result = typeof(object[]).IsCovariantReturnAssignableFrom(typeof(string[,]));
            Assert.False(result);
        }

        [Fact]
        public void IsCovariantReturnAssignableFrom_WithGenericTypeDefinition_ReturnsTrue()
        {
            var result = typeof(IEnumerable<>).IsCovariantReturnAssignableFrom(typeof(IEnumerable<>));
            Assert.True(result);
        }

        [Fact]
        public void GetCovariantReturnMethods_WithTypeWithoutCovariantReturns_ReturnsEmpty()
        {
            var result = typeof(TestService).GetCovariantReturnMethods();
            Assert.Empty(result);
        }

        [Fact]
        public void GetInheritanceDepth_WithDeepInheritance_ReturnsCorrectDepth()
        {
            var result = typeof(DeepDerived).GetInheritanceDepth();
            Assert.Equal(2, result);
        }

        [Fact]
        public void GetInheritanceDepth_WithInterface_ReturnsMinusOne()
        {
            var result = typeof(IDisposable).GetInheritanceDepth();
            Assert.Equal(-1, result);
        }

        public class TestService { }
        public class Base { }
        public class Derived : Base { }
        public class DeepDerived : Derived { }
    }
}
