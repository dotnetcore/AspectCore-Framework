using AspectCore.Lite.Generators;
using Microsoft.AspNetCore.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AspectCore.Lite.Test.Generators
{
    public class InterfaceProxyGeneratorTest
    {
        private readonly IServiceProvider serviceProvider;
        public InterfaceProxyGeneratorTest()
        {
            serviceProvider = DependencyResolver.GetServiceProvider();
        }

        [Fact]
        public void InterfaceProxyGenerator_Constructor_Test()
        {
            var interfaceProxyGenerator = new InterfaceProxyGenerator(serviceProvider, typeof(IInterfaceProxyGeneratorTest));
            Assert.NotNull(interfaceProxyGenerator);
            ExceptionAssert.ThrowsArgumentNull(() => new InterfaceProxyGenerator(null, typeof(IInterfaceProxyGeneratorTest)), "serviceProvider");
            ExceptionAssert.ThrowsArgumentNull(() => new InterfaceProxyGenerator(serviceProvider, null), "interfaceType");
            ExceptionAssert.ThrowsArgument(() => new InterfaceProxyGenerator(serviceProvider, typeof(object)), "interfaceType", "Type should be interface.");
        }

        [Fact]
        public void InterfaceProxyGenerator_TypeBuilder_Throw_Test()
        {
            var interfaceProxyGenerator = new InterfaceProxyGenerator(serviceProvider, typeof(IInterfaceProxyGeneratorTest));
            ExceptionAssert.Throws<InvalidOperationException>(() => interfaceProxyGenerator.TypeBuilder, $"The proxy of {typeof(IInterfaceProxyGeneratorTest).FullName} is not generated.");
        }

        [Fact]
        public void InterfaceProxyGenerator_GenerateProxyType_Test()
        {
            var interfaceProxyGenerator = new InterfaceProxyGenerator(serviceProvider, typeof(IInterfaceProxyGeneratorTest));
            var type = interfaceProxyGenerator.GenerateProxyType();
            var instance = (IInterfaceProxyGeneratorTest)Activator.CreateInstance(type, serviceProvider, new InterfaceProxyGeneratorProxy());
            instance.Id = "proxy";
            Assert.Equal(instance.Id, "proxy");
            Assert.Equal(instance.GetName("Test"), "proxyTest");
        }
    }

    public interface IInterfaceProxyGeneratorTest
    {
        string Id { get; set; }

        string GetName(string name);
    }

    public class InterfaceProxyGeneratorProxy : IInterfaceProxyGeneratorTest
    {
        public string Id { get; set; }

        public string GetName(string name)
        {
            return Id + name;
        }
    }
}
