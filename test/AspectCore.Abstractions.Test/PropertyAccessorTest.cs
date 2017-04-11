using AspectCore.Abstractions.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace AspectCore.Abstractions.Test
{
    public class PropertyAccessorTest
    {
        public string Name { get; set; }

        [Fact]
        public void Setter_Test()
        {
            var property = typeof(PropertyAccessorTest).GetTypeInfo().GetProperty("Name");

            new PropertyAccessor(property).CreatePropertySetter()(this, "Test");

            Assert.Equal(Name, "Test");
        }

        [Fact]
        public void Getter_Test()
        {
            var property = typeof(PropertyAccessorTest).GetTypeInfo().GetProperty("Name");

            Name = "Test";

            var name = new PropertyAccessor(property).CreatePropertyGetter()(this);

            Assert.Equal(Name, name);
        }

    }
}
