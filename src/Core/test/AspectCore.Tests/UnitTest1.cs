using System;
using System.Threading.Tasks;
using AspectCore.Abstractions;
using AspectCore.Core.DynamicProxy;
using AspectCore.Tests.Fakes;
using Xunit;

namespace AspectCore.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            ProxyTypeGenerator generator = new ProxyTypeGenerator(AspectValidatorBuilderFactory.Create());
            var proxyType = generator.CreateInterfaceProxyType(typeof(IService), typeof(Service));
            var instance = (IService)Activator.CreateInstance(proxyType, AspectActivatorFactoryFactory.Create(), new Service());
            instance.Foo();
            instance.Name = "lemon";
            Assert.Equal("lemon", instance.Name);
        }

        [Fact]
        public void Test2()
        {
            ProxyTypeGenerator generator = new ProxyTypeGenerator(AspectValidatorBuilderFactory.Create());
            var proxyType = generator.CreateClassProxyType(typeof(AbsService), typeof(AbsService));
            var instance = (AbsService)Activator.CreateInstance(proxyType, AspectActivatorFactoryFactory.Create());
            instance.Foo();
            instance.Name = "lemon";
            Assert.Equal("lemon", instance.Name);
        }

        [Fact]
        public void Test3()
        {
            ProxyTypeGenerator generator = new ProxyTypeGenerator(AspectValidatorBuilderFactory.Create());
            var proxyType = generator.CreateInterfaceProxyType(typeof(IService));
            var instance = (IService)Activator.CreateInstance(proxyType, AspectActivatorFactoryFactory.Create());
            instance.Foo();
            instance.Name = "lemon";
            //Assert.Equal("lemon", instance.Name);
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

    public class AbsService
    {
        public virtual String Name { get; set; }

        public int Age { get; set; }

        public virtual void Foo() { }
    }
}
