using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AspectCore.Lite.Abstractions.Test
{
    public class ReturnParameterDescriptorTest
    {
        [Fact]
        public void Value_Set_Void()
        {
            var returnParemater = new ReturnParameterDescriptor(null, DescriptorWithParameter.ReturnParameter);
            returnParemater.Value = new object();
            Assert.Null(returnParemater.Value);
        }

        [Fact]
        public void Value_Get_Void()
        {
            var returnParemater = new ReturnParameterDescriptor(new object(), DescriptorWithParameter.ReturnParameter);
            Assert.Null(returnParemater.Value);
        }
    }
}
