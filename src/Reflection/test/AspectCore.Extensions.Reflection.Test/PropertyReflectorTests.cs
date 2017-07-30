using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit;

namespace AspectCore.Extensions.Reflection.Test
{
    public class PropertyReflectorTests
    {
        [Fact]
        public void InstanceProperty_Get_Test()
        {
            var fakes = new PropertyFakes();
            fakes.InstanceProperty = "InstanceProperty";
            var Property = typeof(PropertyFakes).GetTypeInfo().GetProperty("InstanceProperty");
            var PropertyReflector = Property.GetReflector();
            var PropertyValue = PropertyReflector.GetValue(fakes);
            Assert.Equal("InstanceProperty", PropertyValue);
        }

        [Fact]
        public void InstanceProperty_Set_Test()
        {
            var fakes = new PropertyFakes();
            var Property = typeof(PropertyFakes).GetTypeInfo().GetProperty("InstanceProperty");
            var PropertyReflector = Property.GetReflector();
            PropertyReflector.SetValue(fakes, "InstanceProperty");
            Assert.Equal("InstanceProperty", fakes.InstanceProperty);
        }

        [Fact]
        public void StaticProperty_Get_Test()
        {
            PropertyFakes.StaticProperty = "StaticProperty";
            var Property = typeof(PropertyFakes).GetTypeInfo().GetProperty("StaticProperty");
            var PropertyReflector = Property.GetReflector();
            var PropertyValue = PropertyReflector.GetStaticValue();
            Assert.Equal("StaticProperty", PropertyValue);
        }

        [Fact]
        public void StaticProperty_Set_Test()
        {
            var Property = typeof(PropertyFakes).GetTypeInfo().GetProperty("StaticProperty");
            var PropertyReflector = Property.GetReflector();
            PropertyReflector.SetStaticValue("StaticProperty");
            Assert.Equal("StaticProperty", PropertyFakes.StaticProperty);
        }

        [Fact]
        public void StaticProperty_OfT_Test()
        {
            var Property = typeof(PropertyFakes<string>).GetTypeInfo().GetProperty("StaticProperty");
            var PropertyReflector = Property.GetReflector();
            PropertyReflector.SetStaticValue("StaticProperty");
            Assert.Equal("StaticProperty", PropertyReflector.GetStaticValue());
        }     
    }

    public class PropertyFakes
    {
        public static string StaticProperty { get; set; }

        public string InstanceProperty { get; set; }
    }

    public class PropertyFakes<T>
    {
        public static T StaticProperty { get; set; }

        public T InstanceProperty { get; set; }
    }
}
