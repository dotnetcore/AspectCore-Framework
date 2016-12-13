using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace AspectCore.Lite.Abstractions.Test
{
    public class ParameterCollectionTest
    {
        [Fact]
        public void IndexedProperty_Index_Get()
        {
            ParameterCollection collection = new ParameterCollection(new object[] { 0, "L", null, }, DescriptorWithParameter.Parameters);
            Assert.Equal(collection[1].Name, "name");
            Assert.Equal(collection[1].Value, "L");
            Assert.Equal(collection[1].ParameterType, typeof(string));
        }

        [Theory]
        [InlineData("name")]
        public void IndexedProperty_Name(string name)
        {
            ParameterCollection collection = new ParameterCollection(new object[] { 0, "L", null, }, DescriptorWithParameter.Parameters);
            Assert.Equal(collection[name].Name, "name");
            Assert.Equal(collection[name].Value, "L");
            Assert.Equal(collection[name].ParameterType, typeof(string));
        }


        [Fact]
        public void GetEnumerator()
        {
            ParameterCollection collection = new ParameterCollection(new object[] { "L", 0, null, }, DescriptorWithParameter.Parameters);

            foreach (var entry in ((IEnumerable<ParameterDescriptor>)collection))
            {
                Assert.IsType(typeof(ParameterDescriptor), entry);
            }

            foreach (var entry in ((IEnumerable)collection))
            {
                Assert.IsType(typeof(ParameterDescriptor), entry);
            }
        }

        [Theory]
        [InlineData("name", "L")]
        public void Count(string name, object value)
        {
            ParameterCollection collection = new ParameterCollection(new object[] { "L", 0, null }, DescriptorWithParameter.Parameters);
            Assert.Equal(3, collection.Count);
        }

        [Fact]
        public void Ctor_ThrowsArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
              {
                  ParameterCollection collection = new ParameterCollection(new object[] { null }, Array.Empty<ParameterInfo>());
              });
            Assert.Equal("The number of parameters must equal the number of parameterInfos.", ex.Message);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        public void IndexedProperty_Index_With_0_ThrowsArgumentOutOfRangeException(int index)
        {
            ParameterCollection collection = new ParameterCollection(Array.Empty<object>(), Array.Empty<ParameterInfo>());
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => { object entry = collection[index]; } );
            Assert.Equal("index value out of range.\r\nParameter name: index", ex.Message);
        }
    }
}
