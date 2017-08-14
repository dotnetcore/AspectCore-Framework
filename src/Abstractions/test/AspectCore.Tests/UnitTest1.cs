using System;
using AspectCore.Core;
using AspectCore.Tests.Fakes;
using Xunit;

namespace AspectCore.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            ProxyGenerator generator = new ProxyGenerator(AspectValidatorFactory.Create());
            var proxyType = generator.CreateInterfaceProxyType(typeof(IService), typeof(Service));
        }
    }

    [MyInterceptor]
    public interface IService
    {
        void Foo();
    }

    public class Service : IService
    {
        public void Foo()
        {
            throw new NotImplementedException();
        }
    }

    public class MyInterceptor : AspectCore.Abstractions.InterceptorAttribute
    {

    }
}
