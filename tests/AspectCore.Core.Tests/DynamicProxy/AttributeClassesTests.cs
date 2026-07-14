using System;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class AspectCoreSourceGeneratedProxyRegistryAttributeTests
    {
        [Fact]
        public void Constructor_WithNullRegistryType_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new AspectCoreSourceGeneratedProxyRegistryAttribute(null));
            Assert.Equal("registryType", ex.ParamName);
        }

        [Fact]
        public void Constructor_WithValidRegistryType_StoresRegistryType()
        {
            var attribute = new AspectCoreSourceGeneratedProxyRegistryAttribute(typeof(TestRegistry));
            Assert.Equal(typeof(TestRegistry), attribute.RegistryType);
        }

        [Fact]
        public void Constructor_WithValidRegistryType_DoesNotThrow()
        {
            var attribute = new AspectCoreSourceGeneratedProxyRegistryAttribute(typeof(TestRegistry));
            Assert.NotNull(attribute);
        }

        private class TestRegistry
        {
        }
    }

    public class AsyncAspectAttributeTests
    {
        [Fact]
        public void Constructor_DoesNotThrow()
        {
            var attribute = new AsyncAspectAttribute();
            Assert.NotNull(attribute);
        }

        [Fact]
        public void IsAttribute()
        {
            var attribute = new AsyncAspectAttribute();
            Assert.IsAssignableFrom<Attribute>(attribute);
        }

        [Fact]
        public void AttributeUsage_IsOnMethod()
        {
            var usage = typeof(AsyncAspectAttribute).GetCustomAttributes(typeof(AttributeUsageAttribute), false);
            Assert.Single(usage);
            var attr = (AttributeUsageAttribute)usage[0];
            Assert.Equal(AttributeTargets.Method, attr.ValidOn);
        }

        [Fact]
        public void AttributeUsage_AllowMultipleIsFalse()
        {
            var usage = typeof(AsyncAspectAttribute).GetCustomAttributes(typeof(AttributeUsageAttribute), false);
            var attr = (AttributeUsageAttribute)usage[0];
            Assert.False(attr.AllowMultiple);
        }

        [Fact]
        public void AttributeUsage_InheritedIsTrue()
        {
            var usage = typeof(AsyncAspectAttribute).GetCustomAttributes(typeof(AttributeUsageAttribute), false);
            var attr = (AttributeUsageAttribute)usage[0];
            Assert.True(attr.Inherited);
        }
    }

    public class NonAspectAttributeTests
    {
        [Fact]
        public void Constructor_DoesNotThrow()
        {
            var attribute = new NonAspectAttribute();
            Assert.NotNull(attribute);
        }

        [Fact]
        public void IsAttribute()
        {
            var attribute = new NonAspectAttribute();
            Assert.IsAssignableFrom<Attribute>(attribute);
        }

        [Fact]
        public void AttributeUsage_AllowMultipleIsFalse()
        {
            var usage = typeof(NonAspectAttribute).GetCustomAttributes(typeof(AttributeUsageAttribute), false);
            Assert.Single(usage);
            var attr = (AttributeUsageAttribute)usage[0];
            Assert.False(attr.AllowMultiple);
        }

        [Fact]
        public void AttributeUsage_InheritedIsFalse()
        {
            var usage = typeof(NonAspectAttribute).GetCustomAttributes(typeof(AttributeUsageAttribute), false);
            var attr = (AttributeUsageAttribute)usage[0];
            Assert.False(attr.Inherited);
        }
    }

    public class DynamicallyAttributeTests
    {
        [Fact]
        public void Constructor_DoesNotThrow()
        {
            var attribute = new DynamicallyAttribute();
            Assert.NotNull(attribute);
        }

        [Fact]
        public void IsAttribute()
        {
            var attribute = new DynamicallyAttribute();
            Assert.IsAssignableFrom<Attribute>(attribute);
        }

        [Fact]
        public void IsSealed()
        {
            Assert.True(typeof(DynamicallyAttribute).IsSealed);
        }

        [Fact]
        public void AttributeUsage_TargetsClass()
        {
            var usage = typeof(DynamicallyAttribute).GetCustomAttributes(typeof(AttributeUsageAttribute), false);
            Assert.Single(usage);
            var attr = (AttributeUsageAttribute)usage[0];
            Assert.Equal(AttributeTargets.Class, attr.ValidOn);
        }
    }

    public class FromServiceContextAttributeTests
    {
        [Fact]
        public void Constructor_DoesNotThrow()
        {
            var attribute = new FromServiceContextAttribute();
            Assert.NotNull(attribute);
        }

        [Fact]
        public void IsAttribute()
        {
            var attribute = new FromServiceContextAttribute();
            Assert.IsAssignableFrom<Attribute>(attribute);
        }

        [Fact]
        public void AttributeUsage_AllowMultipleIsFalse()
        {
            var usage = typeof(FromServiceContextAttribute).GetCustomAttributes(typeof(AttributeUsageAttribute), false);
            Assert.Single(usage);
            var attr = (AttributeUsageAttribute)usage[0];
            Assert.False(attr.AllowMultiple);
        }
    }
}
