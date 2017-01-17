using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace AspectCore.Abstractions.Test
{
    public class ParameterDescriptorTest
    {
        [Fact]
        public void Value_Set_ReferenceType()
        {
            ParameterInfo info = DescriptorWithParameter.Parameters[2];
            ParameterDescriptor descriptor = new ParameterDescriptor(null, info);
            DescriptorWithParameter service = new DescriptorWithParameter();
            descriptor.Value = service;
            Assert.Equal(service, descriptor.Value);
        }

        [Fact]
        public void Value_Set_AssignableFrom_ReferenceType()
        {
            ParameterInfo info = DescriptorWithParameter.Parameters[2];
            ParameterDescriptor descriptor = new ParameterDescriptor(null, info);
            DescriptorWithParameter service = new DescriptorWithParameter();
            descriptor.Value = service;
            Assert.Equal(service, descriptor.Value);
        }

        [Fact]
        public void Properties_Get()
        {
            ParameterInfo info = DescriptorWithParameter.Parameters[1];
            ParameterDescriptor descriptor = new ParameterDescriptor("LLL", info);

            Assert.NotNull(descriptor);
            Assert.Equal(descriptor.MetaDataInfo, info);
            Assert.Equal(descriptor.Name, info.Name);
            Assert.Equal(descriptor.Value, "LLL");
            Assert.Equal(descriptor.ParameterType, info.ParameterType);
            Assert.Equal(descriptor.CustomAttributes.Length, info.GetCustomAttributes().Count());
        }

        [Theory]
        [InlineData("name", 123)]
        [InlineData("name", typeof(string))]
        public void Value_Set_ThrowInvalidOperationException0(string name, object value)
        {
            ParameterInfo info = DescriptorWithParameter.Parameters[1];
            ParameterDescriptor descriptor = new ParameterDescriptor("LLL", info);
            var exception = Assert.Throws<InvalidOperationException>(() => { descriptor.Value = value; });
            Assert.Equal($"object type are not equal \"{name}\" parameter type or not a derived type of parameter type.", exception.Message);
        }

        [Theory]
        [InlineData("age", "123")]
        [InlineData("age", null)]
        public void Value_Set_ThrowInvalidOperationException1(string name, object value)
        {
            ParameterInfo info = DescriptorWithParameter.Parameters[0];
            ParameterDescriptor descriptor = new ParameterDescriptor(1, info);
            var exception = Assert.Throws<InvalidOperationException>(() => { descriptor.Value = value; });
            Assert.Equal($"object type are not equal \"{name}\" parameter type or not a derived type of parameter type.", exception.Message);
        }

    }
}
