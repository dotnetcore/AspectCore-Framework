using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace AspectCore.Lite.Abstractions.Test
{
    public class TargetDescriptorTest
    {
        [Fact]
        public void Test_Property()
        {
            object impInstance = new DescriptorWithParameter();

            TargetDescriptor descriptor = new TargetDescriptor(impInstance, DescriptorWithParameter.Method, typeof(DescriptorWithParameter), DescriptorWithParameter.Method, typeof(DescriptorWithParameter));

            Assert.Equal(descriptor.ImplementationInstance, impInstance);
            Assert.Equal(descriptor.ServiceMethod, DescriptorWithParameter.Method);
            Assert.Equal(descriptor.ServiceType, typeof(DescriptorWithParameter));
            Assert.Equal(descriptor.ImplementationMethod, DescriptorWithParameter.Method);
            Assert.Equal(descriptor.ImplementationType, typeof(DescriptorWithParameter));
        }

        [Theory]
        [InlineData(1)]
        [InlineData("test")]
        [InlineData(typeof(string))]
        public void Invoke_Test(object value)
        {
            object impInstance = new DescriptorWithParameter();

            TargetDescriptor descriptor = new TargetDescriptor(impInstance, DescriptorWithParameter.Method, typeof(DescriptorWithParameter), DescriptorWithParameter.InvokeMethod, typeof(DescriptorWithParameter));

            var parameters = new ParameterCollection(new object[] { value }, DescriptorWithParameter.InvokeMethod.GetParameters());

            var result = descriptor.Invoke(parameters);

            Assert.Equal(result, value);
        }
    }
}
