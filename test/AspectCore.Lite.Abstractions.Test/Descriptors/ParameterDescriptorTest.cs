using AspectCore.Lite.Abstractions.Descriptors;
using AspectCore.Lite.Abstractions.Test.Fakes;
using Microsoft.AspNetCore.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace AspectCore.Lite.Abstractions.Test.Descriptors
{
    public class ParameterDescriptorTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNullException()
        {
            ExceptionAssert.ThrowsArgumentNull(() => new ParameterDescriptor(null, null), "parameterInfo");
        }


        [Theory]
        [InlineData("name", 123)]
        [InlineData("name", typeof(string))]
        public void Value_Set_ThrowInvalidOperationException0(string name, object value)
        {
            ParameterInfo info = MeaninglessService.Parameters[0];
            ParameterDescriptor descriptor = new ParameterDescriptor("LLL", info);
            ExceptionAssert.Throws<InvalidOperationException>(() => { descriptor.Value = value; }, $"object type are not equal \"{name}\" parameter type or not a derived type of parameter type.");
        }

        [Theory]
        [InlineData("count", "123")]
        [InlineData("count", null)]
        public void Value_Set_ThrowInvalidOperationException1(string name, object value)
        {
            ParameterInfo info = MeaninglessService.Parameters[1];
            ParameterDescriptor descriptor = new ParameterDescriptor(1, info);
            ExceptionAssert.Throws<InvalidOperationException>(() => { descriptor.Value = value; }, $"object type are not equal \"{name}\" parameter type or not a derived type of parameter type.");
        }

        [Theory]
        [InlineData("service", "123")]
        public void Value_Set_ThrowInvalidOperationException2(string name, object value)
        {
            ParameterInfo info = MeaninglessService.Parameters[2];
            ParameterDescriptor descriptor = new ParameterDescriptor(null, info);
            ExceptionAssert.Throws<InvalidOperationException>(() => { descriptor.Value = value; }, $"object type are not equal \"{name}\" parameter type or not a derived type of parameter type.");
        }

        [Fact]
        public void Value_Set_ReferenceType()
        {
            ParameterInfo info = MeaninglessService.Parameters[2];
            ParameterDescriptor descriptor = new ParameterDescriptor(null, info);
            MeaninglessService service = new MeaninglessService();
            descriptor.Value = service;
            Assert.Equal(service, descriptor.Value);
        }

        [Fact]
        public void Value_Set_AssignableFrom_ReferenceType()
        {
            ParameterInfo info = MeaninglessService.Parameters[3];
            ParameterDescriptor descriptor = new ParameterDescriptor(null, info);
            MeaninglessService service = new MeaninglessService();
            descriptor.Value = service;
            Assert.Equal(service, descriptor.Value);
        }

        //[Theory]
        //[InlineData("string", "L")]
        //[InlineData("int", 21)]
        //[InlineData("float", 1.00f)]
        //[InlineData("short", (short)21)]
        //[InlineData("double", 1.00d)]
        //[InlineData("byte", (byte)0)]
        //[InlineData("isBool", true)]
        //[InlineData("char", 'a')]
        //[InlineData("null", null)]
        //public void Ctor_primitiveParameter(string name, object value)
        //{
        //    ParameterEntry parameterEntry = new ParameterEntry(name, value);
        //    Assert.Equal(name, parameterEntry.Name);
        //    Assert.Equal(value, parameterEntry.Value);
        //}

        [Fact]
        public void Properties_Get()
        {
            ParameterInfo info = MeaninglessService.Parameters[0];
            ParameterDescriptor descriptor = new ParameterDescriptor("LLL", info);

            Assert.NotNull(descriptor);
            Assert.Equal(descriptor.MetaDataInfo, info);
            Assert.Equal(descriptor.Name, info.Name);
            Assert.Equal(descriptor.Value, "LLL");
            Assert.Equal(descriptor.ParamterType, info.ParameterType);
            Assert.Equal(descriptor.CustomAttributes.Length, info.GetCustomAttributes().Count());
        }
    }
}
