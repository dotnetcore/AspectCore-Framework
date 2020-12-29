using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.LightInject;
using LightInject;
using Xunit;

namespace AspectCoreTest.LightInject
{
    public class RegistryTests
    {
        public const int Result = 9;

        public interface IService
        {
            [AsyncIncreament]
            int Foo();
        }
        public class Service : IService
        {
            [AsyncIncreament]
            public virtual int Foo() => Result;
        }
        public class ServiceWithRef : IService
        {
            private readonly IService _service;

            public ServiceWithRef(IService service)
            {
                _service = service;
            }

            [AsyncIncreament]
            public virtual int Foo() => _service.Foo();
        }

        private static IServiceContainer CreateContainer()
        {
            return new ServiceContainer().RegisterDynamicProxy();
        }

        [Fact]
        public void Register_Interface()
        {
            var container = CreateContainer();
            container.Register<IService, Service>(new PerRequestLifeTime());
            var inter = container.GetInstance<IService>();
            Assert.Equal(Result + 1, inter.Foo());
        }

        [Fact]
        public void Register_Self()
        {
            var container = CreateContainer();
            container.Register<Service>(new PerRequestLifeTime());
            var obj = container.GetInstance<Service>();
            Assert.Equal(Result + 1, obj.Foo());
        }

        [Fact]
        public void Register_Instance()
        {
            var container = CreateContainer();
            var service = new Service();
            container.RegisterInstance<IService>(service);
            container.RegisterInstance<Service>(service);

            var inter = container.GetInstance<IService>();
            Assert.Equal(Result + 1, inter.Foo());
            Assert.Same(inter, container.GetInstance<IService>());

            var obj = container.GetInstance<Service>();
            Assert.Equal(Result + 1, obj.Foo());
            Assert.Same(obj, container.GetInstance<Service>());
        }

        [Fact]
        public void RegisterInterface_Factory()
        {
            var container = CreateContainer();
            container.Register<IService>(s => new Service(), new PerRequestLifeTime());

            var inter = container.GetInstance<IService>();
            Assert.Equal(Result + 1, inter.Foo());
        }

        [Fact]
        public void RegisterClass_Factory()
        {
            var container = CreateContainer();
            container.Register<Service>(s => new Service(), new PerRequestLifeTime());

            var obj = container.GetInstance<Service>();
            Assert.Equal(Result + 1, obj.Foo());
        }

        [Fact]
        public void RegisterInterface_WithRef_Factory()
        {
            var container = CreateContainer();
            container.Register<IService>(s => new ServiceWithRef(new Service()), new PerRequestLifeTime());

            var obj = container.GetInstance<IService>();
            Assert.Equal(Result + 1, obj.Foo());
        }

        [Fact]
        public void RegisterInterface_WithRefFromContainer_Factory()
        {
            var container = CreateContainer();
            container.Register<Service>(s => new Service(), new PerRequestLifeTime());
            container.Register<IService>(s => new ServiceWithRef(s.GetInstance<Service>()), new PerRequestLifeTime());

            var obj = container.GetInstance<IService>();
            Assert.Equal(Result + 1 + 1, obj.Foo());
        }

        [Fact]
        public void Register_Factory_SameFactoryBody_AreDifferent()
        {
            var container = CreateContainer();
            container.Register<IService>(s => new Service(), new PerRequestLifeTime());
            container.Register<Service>(s => new Service(), new PerRequestLifeTime());
            var inter = container.GetInstance<IService>();
            var obj = container.GetInstance<Service>();
            Assert.NotSame(inter, obj);
        }

        [Fact]
        public void Register_Factory_Transient_AreDifferent()
        {
            var container = CreateContainer();
            container.Register<IService>(s => new Service());

            var service1 = container.GetInstance<IService>();
            var service2 = container.GetInstance<IService>();
            Assert.False(ReferenceEquals(service1, service2));
        }

        [Fact]
        public void Register_Factory_Singleton_AreSame()
        {
            var container = CreateContainer();
            container.Register<IService>(s => new Service(), new PerContainerLifetime());

            var service1 = container.GetInstance<IService>();
            var service2 = container.GetInstance<IService>();
            Assert.True(ReferenceEquals(service1, service2));
        }
    }
}
