using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AspectCore.DependencyInjection;
using AspectCore.Utils;
using Xunit;

namespace AspectCore.Core.Tests.Utils
{
    public class ActivatorUtilsTests
    {
        [Fact]
        public void CreateManyEnumerable_WithElementType_ReturnsNonNullInstance()
        {
            var result = ActivatorUtils.CreateManyEnumerable(typeof(string));

            Assert.NotNull(result);
            Assert.IsType<ManyEnumerable<string>>(result);
        }

        [Fact]
        public void CreateManyEnumerable_WithElementType_ReturnsEmptyEnumerable()
        {
            var result = ActivatorUtils.CreateManyEnumerable(typeof(int));

            var enumerable = Assert.IsAssignableFrom<IEnumerable<int>>(result);
            Assert.Empty(enumerable);
        }

        [Fact]
        public void CreateManyEnumerable_WithElementType_ReturnsCorrectGenericType()
        {
            var result = ActivatorUtils.CreateManyEnumerable(typeof(Guid));

            Assert.IsType<ManyEnumerable<Guid>>(result);
        }

        [Fact]
        public void CreateManyEnumerable_WithElementTypeAndArray_ReturnsNonNullInstance()
        {
            var array = new[] { "a", "b", "c" };

            var result = ActivatorUtils.CreateManyEnumerable(typeof(string), array);

            Assert.NotNull(result);
            Assert.IsType<ManyEnumerable<string>>(result);
        }

        [Fact]
        public void CreateManyEnumerable_WithElementTypeAndArray_ReturnsAllElements()
        {
            var array = new[] { 1, 2, 3, 4, 5 };

            var result = ActivatorUtils.CreateManyEnumerable(typeof(int), array);

            var enumerable = Assert.IsAssignableFrom<IEnumerable<int>>(result);
            Assert.Equal(array, enumerable.ToArray());
        }

        [Fact]
        public void CreateManyEnumerable_WithElementTypeAndEmptyArray_ReturnsEmptyEnumerable()
        {
            var array = Array.Empty<object>();

            var result = ActivatorUtils.CreateManyEnumerable(typeof(object), array);

            var enumerable = Assert.IsAssignableFrom<IEnumerable<object>>(result);
            Assert.Empty(enumerable);
        }

        [Fact]
        public void CreateManyEnumerable_WithElementTypeAndArray_PreservesOrder()
        {
            var array = new[] { 10, 20, 30 };

            var result = ActivatorUtils.CreateManyEnumerable(typeof(int), array);

            var enumerable = Assert.IsAssignableFrom<IEnumerable<int>>(result);
            Assert.Equal(new[] { 10, 20, 30 }, enumerable.ToArray());
        }

        [Fact]
        public void CreateManyEnumerable_WithReferenceType_WorksCorrectly()
        {
            var array = new[] { new object(), new object() };

            var result = ActivatorUtils.CreateManyEnumerable(typeof(object), array);

            var enumerable = Assert.IsAssignableFrom<IEnumerable<object>>(result);
            Assert.Equal(2, enumerable.Count());
        }

        [Fact]
        public void CreateManyEnumerable_ResultIsEnumerable()
        {
            var result = ActivatorUtils.CreateManyEnumerable(typeof(string));

            Assert.IsAssignableFrom<IEnumerable>(result);
        }

        [Fact]
        public void CreateManyEnumerable_ResultIsIManyEnumerable()
        {
            var result = ActivatorUtils.CreateManyEnumerable(typeof(string));

            Assert.IsAssignableFrom<IManyEnumerable<string>>(result);
        }

        [Fact]
        public void CreateManyEnumerable_WithNullElementType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ActivatorUtils.CreateManyEnumerable(null));
        }

        [Fact]
        public void CreateManyEnumerable_WithNullArray_CreatesInstanceWithNull()
        {
            var result = ActivatorUtils.CreateManyEnumerable(typeof(string), null);

            Assert.NotNull(result);
            Assert.IsType<ManyEnumerable<string>>(result);
        }

        [Fact]
        public void CreateManyEnumerable_WithNullElementTypeAndArray_ThrowsArgumentNullException()
        {
            var array = new[] { "a" };
            Assert.Throws<ArgumentNullException>(() => ActivatorUtils.CreateManyEnumerable(null, array));
        }
    }
}
