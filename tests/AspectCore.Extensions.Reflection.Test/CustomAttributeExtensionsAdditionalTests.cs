using System;
using System.Reflection;
using Xunit;

namespace AspectCore.Extensions.Reflection.Test
{
    public class CustomAttributeExtensionsAdditionalTests
    {
        [Fact]
        public void GetCustomAttributes_Null_Provider_Throws()
        {
            ICustomAttributeReflectorProvider provider = null;
            Assert.Throws<ArgumentNullException>(() => provider.GetCustomAttributes());
        }

        [Fact]
        public void GetCustomAttributes_With_Type_Null_Provider_Throws()
        {
            ICustomAttributeReflectorProvider provider = null;
            Assert.Throws<ArgumentNullException>(() => provider.GetCustomAttributes(typeof(Attribute)));
        }

        [Fact]
        public void GetCustomAttributes_With_Type_Null_Type_Throws()
        {
            var reflector = typeof(CustomAttributeExtensionsTests).GetTypeInfo().GetReflector();
            Assert.Throws<ArgumentNullException>(() => reflector.GetCustomAttributes((Type)null));
        }

        [Fact]
        public void GetCustomAttributes_Generic_Null_Provider_Throws()
        {
            ICustomAttributeReflectorProvider provider = null;
            Assert.Throws<ArgumentNullException>(() => provider.GetCustomAttributes<Attribute>());
        }

        [Fact]
        public void GetCustomAttribute_Null_Provider_Throws()
        {
            ICustomAttributeReflectorProvider provider = null;
            Assert.Throws<ArgumentNullException>(() => provider.GetCustomAttribute(typeof(Attribute)));
        }

        [Fact]
        public void GetCustomAttribute_Null_Type_Throws()
        {
            var reflector = typeof(CustomAttributeExtensionsTests).GetTypeInfo().GetReflector();
            Assert.Throws<ArgumentNullException>(() => reflector.GetCustomAttribute((Type)null));
        }

        [Fact]
        public void GetCustomAttribute_Generic_Null_Provider_Throws()
        {
            ICustomAttributeReflectorProvider provider = null;
            Assert.Throws<ArgumentNullException>(() => provider.GetCustomAttribute<Attribute>());
        }

        [Fact]
        public void IsDefined_Null_Provider_Throws()
        {
            ICustomAttributeReflectorProvider provider = null;
            Assert.Throws<ArgumentNullException>(() => provider.IsDefined(typeof(Attribute)));
        }

        [Fact]
        public void IsDefined_Null_Type_Throws()
        {
            var reflector = typeof(CustomAttributeExtensionsTests).GetTypeInfo().GetReflector();
            Assert.Throws<ArgumentNullException>(() => reflector.IsDefined((Type)null));
        }

        [Fact]
        public void IsDefined_Generic_Null_Provider_Throws()
        {
            ICustomAttributeReflectorProvider provider = null;
            Assert.Throws<ArgumentNullException>(() => provider.IsDefined<Attribute>());
        }

        [Fact]
        public void GetCustomAttributes_No_Attributes_Returns_Empty()
        {
            var reflector = typeof(NoAttributeClass).GetTypeInfo().GetReflector();
            var attrs = reflector.GetCustomAttributes();
            Assert.Empty(attrs);
        }

        [Fact]
        public void GetCustomAttributes_With_Type_No_Match_Returns_Empty()
        {
            var reflector = typeof(NoAttributeClass).GetTypeInfo().GetReflector();
            var attrs = reflector.GetCustomAttributes(typeof(AttributeFakes));
            Assert.Empty(attrs);
        }

        [Fact]
        public void GetCustomAttributes_Generic_No_Match_Returns_Empty()
        {
            var reflector = typeof(NoAttributeClass).GetTypeInfo().GetReflector();
            var attrs = reflector.GetCustomAttributes<AttributeFakes>();
            Assert.Empty(attrs);
        }

        [Fact]
        public void GetCustomAttribute_Not_Found_Returns_Null()
        {
            var reflector = typeof(NoAttributeClass).GetTypeInfo().GetReflector();
            var attr = reflector.GetCustomAttribute(typeof(AttributeFakes));
            Assert.Null(attr);
        }

        [Fact]
        public void GetCustomAttribute_Generic_Not_Found_Returns_Null()
        {
            var reflector = typeof(NoAttributeClass).GetTypeInfo().GetReflector();
            var attr = reflector.GetCustomAttribute<AttributeFakes>();
            Assert.Null(attr);
        }

        [Fact]
        public void IsDefined_Not_Defined_Returns_False()
        {
            var reflector = typeof(NoAttributeClass).GetTypeInfo().GetReflector();
            Assert.False(reflector.IsDefined(typeof(AttributeFakes)));
        }

        [Fact]
        public void IsDefined_Generic_Not_Defined_Returns_False()
        {
            var reflector = typeof(NoAttributeClass).GetTypeInfo().GetReflector();
            Assert.False(reflector.IsDefined<AttributeFakes>());
        }
    }

    public class NoAttributeClass
    {
    }
}
