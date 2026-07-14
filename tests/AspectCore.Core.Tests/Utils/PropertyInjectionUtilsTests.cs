using System;
using AspectCore.DependencyInjection;
using AspectCore.Utils;
using Xunit;

namespace AspectCore.Core.Tests.Utils
{
    public class PropertyInjectionUtilsTests
    {
        [Fact]
        public void TypeRequired_WithNullType_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => PropertyInjectionUtils.TypeRequired(null));
            Assert.Equal("type", ex.ParamName);
        }

        [Fact]
        public void TypeRequired_WithNoInjectableProperties_ReturnsFalse()
        {
            Assert.False(PropertyInjectionUtils.TypeRequired(typeof(NoPropertiesClass)));
        }

        [Fact]
        public void TypeRequired_WithReadOnlyProperty_ReturnsFalse()
        {
            Assert.False(PropertyInjectionUtils.TypeRequired(typeof(ReadOnlyPropertyClass)));
        }

        [Fact]
        public void TypeRequired_WithWritablePropertyWithoutAttribute_ReturnsFalse()
        {
            Assert.False(PropertyInjectionUtils.TypeRequired(typeof(WritablePropertyNoAttributeClass)));
        }

        [Fact]
        public void TypeRequired_WithWritablePropertyWithAttribute_ReturnsTrue()
        {
            Assert.True(PropertyInjectionUtils.TypeRequired(typeof(WritablePropertyWithAttributeClass)));
        }

        [Fact]
        public void Required_WithNullInstance_ReturnsFalse()
        {
            Assert.False(PropertyInjectionUtils.Required(null));
        }

        [Fact]
        public void Required_WithInstanceNoInjectableProperties_ReturnsFalse()
        {
            Assert.False(PropertyInjectionUtils.Required(new NoPropertiesClass()));
        }

        [Fact]
        public void Required_WithInstanceWithInjectableProperty_ReturnsTrue()
        {
            Assert.True(PropertyInjectionUtils.Required(new WritablePropertyWithAttributeClass()));
        }

        [Fact]
        public void TypeRequired_CachesResult()
        {
            var first = PropertyInjectionUtils.TypeRequired(typeof(WritablePropertyWithAttributeClass));
            var second = PropertyInjectionUtils.TypeRequired(typeof(WritablePropertyWithAttributeClass));
            Assert.Equal(first, second);
        }

        private class NoPropertiesClass { }

        private class ReadOnlyPropertyClass
        {
            [FromServiceContext]
            public string Name => "test";
        }

        private class WritablePropertyNoAttributeClass
        {
            public string Name { get; set; }
        }

        private class WritablePropertyWithAttributeClass
        {
            [FromServiceContext]
            public string Name { get; set; }
        }
    }
}
