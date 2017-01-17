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
        [Theory]
        [InlineData(typeof(ITargetService),typeof(TargetService))]
        [InlineData(typeof(AbsTargetService), typeof(TargetService))]
        public void Dynamically_Test(Type serviceType, Type impType)
        {
            var generator = new ProxyGenerator(new AspectValidator(new AspectConfiguration()));
            var proxyType = generator.CreateType(serviceType, impType);
            Assert.True(proxyType.GetTypeInfo().IsDynamically());
        }
    }
}
