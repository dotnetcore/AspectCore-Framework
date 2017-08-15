using System;
using System.Reflection;
using AspectCore.Abstractions;
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
            ProxyGenerator generator = new ProxyGenerator(AspectValidatorBuilderFactory.Create());
            var proxyType = generator.CreateInterfaceProxyType(typeof(IService), typeof(Service));
            var instance = (IService)Activator.CreateInstance(proxyType, AspectActivatorFactoryFactory.Create(), new Service());
            instance.Foo();
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

    public class ServiceProxy : IService
    {
        private readonly IAspectActivatorFactory _activatorFactory;
        private readonly IService _service;

        public ServiceProxy(IAspectActivatorFactory activatorFactory, IService service)
        {
            _activatorFactory = activatorFactory;
            _service = service;
        }

        public void Foo()
        {
            _activatorFactory.Create().Invoke<object>(new AspectActivatorContext(typeof(IService), Methods.serviceFoo, Methods.impFoo, Methods.targetFoo, _service, this, null));
        }

        internal class Methods
        {
            internal static MethodInfo serviceFoo;
            internal static MethodInfo impFoo;
            internal static MethodInfo targetFoo;
        }
    }
}
