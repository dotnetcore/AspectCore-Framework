using AspectCore.Abstractions.Extensions;
using AspectCore.Abstractions.Resolution.Test.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace AspectCore.Abstractions.Resolution.Test
{
    public class DynamicallyTests
    {
        [Fact]
        public void Dynamically_Test()
        {
            var generator = new ProxyGenerator(new AspectValidator(new AspectConfigure()));
            var proxyType = generator.CreateInterfaceProxyType(typeof(ITargetService), typeof(TargetService));
            Assert.True(proxyType.GetTypeInfo().IsDynamically());
        }

        [Fact]
        public void Dynamically_TestWithClass()
        {
            var generator = new ProxyGenerator(new AspectValidator(new AspectConfigure()));
            var proxyType = generator.CreateClassProxyType(typeof(AbsTargetService), typeof(TargetService));
            Assert.True(proxyType.GetTypeInfo().IsDynamically());
        }
    }
}
