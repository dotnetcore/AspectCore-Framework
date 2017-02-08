using AspectCore.Abstractions.Internal.Test.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace AspectCore.Abstractions.Internal.Test
{
    public class ProxyGeneratorTest
    {
        [Fact]
        public void CreateProxyType_Test()
        {
            var generator = new ProxyGenerator(new AspectValidator(new AspectConfigure()));
            var proxyType = generator.CreateInterfaceProxyType(typeof(ITargetService), typeof(TargetService));
            var proxyInstance = Activator.CreateInstance(proxyType, new InstanceServiceProvider(null), new InstanceServiceProvider(new TargetService()));

            Assert.IsAssignableFrom<ITargetService>(proxyInstance);
        }

        [Fact]
        public void Create_Generic_ProxyType_Test()
        {
            var generator = new ProxyGenerator(new AspectValidator(new AspectConfigure()));
            var proxyType = generator.CreateInterfaceProxyType(typeof(ITargetService<>), typeof(TargetService<>));

            Assert.True(proxyType.GetTypeInfo().IsGenericTypeDefinition);
            Assert.Single(proxyType.GetTypeInfo().GenericTypeParameters);

            var proxyInstance = Activator.CreateInstance(proxyType.MakeGenericType(typeof(object)), new InstanceServiceProvider(null), new InstanceServiceProvider(null));

            Assert.IsAssignableFrom<ITargetService<object>>(proxyInstance);
        }

        [Fact]
        public void CreateProxyType_Cache_Test()
        {
            var generator = new ProxyGenerator(new AspectValidator(new AspectConfigure()));
            var proxyType = generator.CreateInterfaceProxyType(typeof(ITargetService), typeof(TargetService));
            var proxyTypeCache = generator.CreateInterfaceProxyType(typeof(ITargetService), typeof(TargetService));
        }
    }
}
