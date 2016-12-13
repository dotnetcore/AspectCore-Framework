using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AspectCore.Lite.Abstractions.Test
{
    public class ProxyDescriptorTest
    {
        [Fact]
        public void Test_Property()
        {
            object impInstance = new DescriptorWithParameter();

            ProxyDescriptor descriptor = new ProxyDescriptor(impInstance, DescriptorWithParameter.Method, typeof(DescriptorWithParameter));

            Assert.Equal(descriptor.ProxyInstance, impInstance);
            Assert.Equal(descriptor.ProxyMethod, DescriptorWithParameter.Method);
            Assert.Equal(descriptor.ProxyType, typeof(DescriptorWithParameter));
        }
    }
}
