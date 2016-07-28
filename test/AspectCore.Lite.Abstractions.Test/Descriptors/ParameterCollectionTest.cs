using AspectCore.Lite.Abstractions.Descriptors;
using AspectCore.Lite.Abstractions.Test.Fakes;
using Microsoft.AspNetCore.Testing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace AspectCore.Lite.Abstractions.Test.Descriptors
{
    public class ParameterCollectionTest
    {

        [Fact]
        public void Ctor_ThrowsArgumentNullException()
        {
            ExceptionAssert.ThrowsArgumentNull(() => { ParameterCollection collection = new ParameterCollection(null, Array.Empty<ParameterInfo>()); }, "parameters");
            ExceptionAssert.ThrowsArgumentNull(() => { ParameterCollection collection = new ParameterCollection(Array.Empty<object>(), null); }, "parameterInfos");
        }

        [Fact]
        public void Ctor_ThrowsArgumentException()
        {
            ExceptionAssert.ThrowsArgument(() =>
            {
                ParameterCollection collection = new ParameterCollection(new object[] { null }, Array.Empty<ParameterInfo>());
            },
            null, "the number of parameters must equal the number of parameterInfos.");
        }

        [Fact]
        public void IndexedProperty_Index_Get()
        {
            ParameterCollection collection = new ParameterCollection(new object[] { "L", 0, null, null, }, MeaninglessService.Parameters);
            Assert.Equal(collection[0].Name, "name");
            Assert.Equal(collection[0].Value, "L");
            Assert.Equal(collection[0].ParamterType, typeof(string));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        public void IndexedProperty_Index_With_0_ThrowsArgumentOutOfRangeException(int index)
        {
            ParameterCollection collection = new ParameterCollection(Array.Empty<object>(), Array.Empty<ParameterInfo>());
            ExceptionAssert.ThrowsArgumentOutOfRange(() => { object entry = collection[index]; }, nameof(index), "index value out of range.");
        }

        [Theory]
        [InlineData("name")]
        public void IndexedProperty_Name(string name)
        {
            ParameterCollection collection = new ParameterCollection(new object[] { "L", 0, null, null, }, MeaninglessService.Parameters);
            Assert.Equal(collection[name].Name, "name");
            Assert.Equal(collection[name].Value, "L");
            Assert.Equal(collection[name].ParamterType, typeof(string));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void IndexedProperty_Name_Get_ThrowsArgumentNullException(string name)
        {
            ParameterCollection collection = new ParameterCollection(new object[] { "L", 0, null, null, }, MeaninglessService.Parameters);
            ExceptionAssert.ThrowsArgumentNull(() => { var value = collection[name]; }, nameof(name));
        }


        [Theory]
        [InlineData("age")]
        [InlineData("Name")]
        public void IndexedProperty_Name_Get_ThrowsMissingMemberException(string name)
        {
            ParameterCollection collection = new ParameterCollection(new object[] { "L", 0, null, null, }, MeaninglessService.Parameters);
            ExceptionAssert.Throws<MissingMemberException>(() => { var value = collection[name]; }, $"does not exist the parameter nameof \"{name}\".");
        }

        [Fact]
        public void GetEnumerator()
        {
            ParameterCollection collection = new ParameterCollection(new object[] { "L", 0, null, null, }, MeaninglessService.Parameters);

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
            ParameterCollection collection = new ParameterCollection(new object[] { "L", 0, null, null, }, MeaninglessService.Parameters);
            Assert.Equal(4, collection.Count);
        }
    }
}