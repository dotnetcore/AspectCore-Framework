using System;
using System.Reflection;
using System.Threading.Tasks;
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
            instance.Name = "lemon";
            Assert.Equal("lemon", instance.Name);
        }
    }

    [MyInterceptor]
    public interface IService
    {
        string Name { get; set; }
        void Foo();
    }

    public class Service : IService
    {
        public string Name { get; set; }

        public void Foo()
        {
        }
    }

    public class MyInterceptor : AspectCore.Abstractions.InterceptorAttribute
    {
        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            return base.Invoke(context, next);
        }
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

        public string Name { get; set; }

        public void Foo()
        {
            _activatorFactory.Create().Invoke<object>(new AspectActivatorContext( Methods.serviceFoo, Methods.impFoo, Methods.targetFoo, _service, this, null));
        }

        internal class Methods
        {
            internal static MethodInfo serviceFoo;
            internal static MethodInfo impFoo;
            internal static MethodInfo targetFoo;
        }
    }
}
