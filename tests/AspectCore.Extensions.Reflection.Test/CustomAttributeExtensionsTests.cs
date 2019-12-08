using System;
using System.Reflection;
using Xunit;

namespace AspectCore.Extensions.Reflection.Test
{
    [AttributeFakes]
    [AttributeFakes]
    [AttributeFakes1(1)]
    [AttributeFakes1(2)]
    [AttributeFakes2(typeof(object))]
    public class CustomAttributeExtensionsTests
    {
        [Fact]
        public void GetCustomAttributes_Test()
        {
            var reflector = typeof(CustomAttributeExtensionsTests).GetTypeInfo().GetReflector();
            var attrs = reflector.GetCustomAttributes();
            Assert.Equal(5, attrs.Length);
        }

        [Fact]
        public void GetCustomAttributes_With_Attr_Type_Test()
        {
            var reflector = typeof(CustomAttributeExtensionsTests).GetTypeInfo().GetReflector();
            var attrs = reflector.GetCustomAttributes(typeof(AttributeFakes));
            //AttributeFakes1 inherit from AttributeFakes
            Assert.Equal(4, attrs.Length);
            attrs = reflector.GetCustomAttributes<AttributeFakes1>();
            Assert.Equal(2, attrs.Length);
        }

        [Fact]
        public void GetCustomAttribute_Test()
        {
            var reflector = typeof(CustomAttributeExtensionsTests).GetTypeInfo().GetReflector();
            var attr1 = reflector.GetCustomAttribute<AttributeFakes1>();
            Assert.Equal(1, attr1.Id);
            var attr2 = (AttributeFakes2)reflector.GetCustomAttribute(typeof(AttributeFakes2));
            Assert.Equal(typeof(object), attr2.Type);
        }

        [Fact]
        public void IsDefined_Test()
        {
            var reflector = typeof(CustomAttributeExtensionsTests).GetTypeInfo().GetReflector();
            Assert.True(reflector.IsDefined<AttributeFakes>());
            Assert.True(reflector.IsDefined<AttributeFakes2>());
            Assert.True(reflector.IsDefined<Attribute>());
        }
    }
}