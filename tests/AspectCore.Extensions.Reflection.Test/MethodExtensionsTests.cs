using System;
using System.Reflection;
using Xunit;

namespace AspectCore.Extensions.Reflection.Test
{
    public class MethodExtensionsTests
    {
        [Fact]
        public void IsPropertyBinding_Null_Method_Throws()
        {
            MethodInfo method = null;
            Assert.Throws<ArgumentNullException>(() => method.IsPropertyBinding());
        }

        [Fact]
        public void GetBindingProperty_Null_Method_Throws()
        {
            MethodInfo method = null;
            Assert.Throws<ArgumentNullException>(() => method.GetBindingProperty());
        }

        [Fact]
        public void IsPropertyBinding_Property_Getter_Returns_True()
        {
            var method = typeof(FakePropertyClass).GetMethod("get_Name");
            Assert.True(method.IsPropertyBinding());
        }

        [Fact]
        public void IsPropertyBinding_Property_Setter_Returns_True()
        {
            var method = typeof(FakePropertyClass).GetMethod("set_Name");
            Assert.True(method.IsPropertyBinding());
        }

        [Fact]
        public void IsPropertyBinding_Regular_Method_Returns_False()
        {
            var method = typeof(FakePropertyClass).GetMethod("DoSomething");
            Assert.False(method.IsPropertyBinding());
        }

        [Fact]
        public void GetBindingProperty_Property_Getter_Returns_Property()
        {
            var method = typeof(FakePropertyClass).GetMethod("get_Name");
            var property = method.GetBindingProperty();
            Assert.NotNull(property);
            Assert.Equal("Name", property.Name);
        }

        [Fact]
        public void GetBindingProperty_Property_Setter_Returns_Property()
        {
            var method = typeof(FakePropertyClass).GetMethod("set_Name");
            var property = method.GetBindingProperty();
            Assert.NotNull(property);
            Assert.Equal("Name", property.Name);
        }

        [Fact]
        public void GetBindingProperty_Regular_Method_Returns_Null()
        {
            var method = typeof(FakePropertyClass).GetMethod("DoSomething");
            var property = method.GetBindingProperty();
            Assert.Null(property);
        }

        [Fact]
        public void GetBindingProperty_ReadOnly_Property_Getter_Returns_Property()
        {
            var method = typeof(FakePropertyClass).GetMethod("get_ReadOnlyValue");
            var property = method.GetBindingProperty();
            Assert.NotNull(property);
            Assert.Equal("ReadOnlyValue", property.Name);
        }

        [Fact]
        public void GetBindingProperty_WriteOnly_Property_Setter_Returns_Property()
        {
            var method = typeof(FakePropertyClass).GetMethod("set_WriteOnlyValue");
            var property = method.GetBindingProperty();
            Assert.NotNull(property);
            Assert.Equal("WriteOnlyValue", property.Name);
        }

        [Fact]
        public void GetBindingProperty_Caching_Returns_Same_Instance()
        {
            var method = typeof(FakePropertyClass).GetMethod("get_Name");
            var first = method.GetBindingProperty();
            var second = method.GetBindingProperty();
            Assert.Same(first, second);
        }

        [Fact]
        public void IsPropertyBinding_Static_Property_Getter_Returns_True()
        {
            var method = typeof(FakePropertyClass).GetMethod("get_StaticValue");
            Assert.True(method.IsPropertyBinding());
        }

        [Fact]
        public void GetBindingProperty_Static_Property_Getter_Returns_Property()
        {
            var method = typeof(FakePropertyClass).GetMethod("get_StaticValue");
            var property = method.GetBindingProperty();
            Assert.NotNull(property);
            Assert.Equal("StaticValue", property.Name);
        }
    }

    public class FakePropertyClass
    {
        public string Name { get; set; }

        public int ReadOnlyValue { get; }

        public int WriteOnlyValue
        {
            set { }
        }

        public static int StaticValue { get; set; }

        public void DoSomething()
        {
        }
    }
}
