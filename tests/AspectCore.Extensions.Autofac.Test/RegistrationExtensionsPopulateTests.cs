using System;
using System.Collections.Generic;
using AspectCore.DependencyInjection;
using AspectCore.Extensions.Autofac;
using AspectCore.Extensions.Test.Fakes;
using Autofac;
using Xunit;

namespace AspectCoreTest.Autofac
{
    public class RegistrationExtensionsPopulateTests
    {
        [Fact]
        public void Populate_WithTypeServiceDefinition_RegistersService()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();

            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IService), typeof(Service), Lifetime.Transient)
            };

            builder.Populate(services);
            var container = builder.Build();

            var service = container.Resolve<IService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void Populate_WithSingletonLifetime_ReturnsSameInstance()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();

            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IService), typeof(Service), Lifetime.Singleton)
            };

            builder.Populate(services);
            var container = builder.Build();

            var service1 = container.Resolve<IService>();
            var service2 = container.Resolve<IService>();
            Assert.Same(service1, service2);
        }

        [Fact]
        public void Populate_WithScopedLifetime_ReturnsDifferentInstancesPerScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();

            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IService), typeof(Service), Lifetime.Scoped)
            };

            builder.Populate(services);
            var container = builder.Build();

            object scope1Instance;
            object scope2Instance;
            using (var scope1 = container.BeginLifetimeScope())
            {
                scope1Instance = scope1.Resolve<IService>();
            }
            using (var scope2 = container.BeginLifetimeScope())
            {
                scope2Instance = scope2.Resolve<IService>();
            }
            Assert.NotSame(scope1Instance, scope2Instance);
        }

        [Fact]
        public void Populate_WithDelegateServiceDefinition_RegistersService()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();

            var services = new List<ServiceDefinition>
            {
                new DelegateServiceDefinition(typeof(IService), resolver => new Service(), Lifetime.Transient)
            };

            builder.Populate(services);
            var container = builder.Build();

            var service = container.Resolve<IService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void Populate_WithInstanceServiceDefinition_RegistersService()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();

            var instance = new Service();
            var services = new List<ServiceDefinition>
            {
                new InstanceServiceDefinition(typeof(IService), instance)
            };

            builder.Populate(services);
            var container = builder.Build();

            var resolved = container.Resolve<IService>();
            Assert.NotNull(resolved);
            Assert.IsAssignableFrom<IService>(resolved);
        }

        [Fact]
        public void Populate_WithGenericTypeDefinition_RegistersService()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();

            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IGenericService<>), typeof(GenericService<>), Lifetime.Transient)
            };

            builder.Populate(services);
            var container = builder.Build();

            var service = container.Resolve<IGenericService<IService>>();
            Assert.NotNull(service);
        }

        [Fact]
        public void Populate_NullBuilder_Throws()
        {
            ContainerBuilder builder = null;
            var services = new List<ServiceDefinition>();
            Assert.ThrowsAny<Exception>(() => builder.Populate(services));
        }

        [Fact]
        public void Populate_WithMultipleServices_RegistersAll()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();

            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IService), typeof(Service), Lifetime.Transient),
                new DelegateServiceDefinition(typeof(IService2), resolver => new Service2(), Lifetime.Singleton),
                new InstanceServiceDefinition(typeof(IService3), new Service3())
            };

            builder.Populate(services);
            var container = builder.Build();

            Assert.NotNull(container.Resolve<IService>());
            Assert.NotNull(container.Resolve<IService2>());
            Assert.NotNull(container.Resolve<IService3>());
        }

        public interface IService2 { }
        public class Service2 : IService2 { }
        public interface IService3 { }
        public class Service3 : IService3 { }
        public interface IGenericService<T> { }
        public class GenericService<T> : IGenericService<T> { }
    }
}
