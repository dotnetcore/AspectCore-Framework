using System.Reflection;
using AspectCore.Abstractions.Extensions;
using AspectCore.Abstractions.Internal.Test.Fakes;
using AspectCore.Abstractions.Test.Fakes;
using Xunit;

namespace AspectCore.Abstractions.Internal.Test
{
    public class DynamicallyTests
    {
        [Fact]
        public void Dynamically_Test()
        {
            var generator = new ProxyGenerator(AspectValidatorFactory.GetAspectValidator(new AspectConfigure()));
            var proxyType = generator.CreateInterfaceProxyType(typeof(ITargetService), typeof(TargetService));
            Assert.True(proxyType.GetTypeInfo().IsProxyType());
        }

        [Fact]
        public void Dynamically_TestWithClass()
        {
            var generator = new ProxyGenerator(AspectValidatorFactory.GetAspectValidator(new AspectConfigure()));
            var proxyType = generator.CreateClassProxyType(typeof(AbsTargetService), typeof(TargetService));
            Assert.True(proxyType.GetTypeInfo().IsProxyType());
        }
    }
}
