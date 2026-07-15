using System;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Autofac;
using AspectCore.Extensions.Test.Fakes;
using Autofac;
using Xunit;

namespace AspectCoreTest.Autofac
{
    public class ActivationResolveMiddlewareAdditionalTests
    {
        private static IContainer BuildContainer(Action<ContainerBuilder> beforeBuild = null)
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();
            builder.RegisterType<Service>().As<IService>();
            beforeBuild?.Invoke(builder);
            return builder.Build();
        }

        [Fact]
        public void Resolve_InterfaceService_CreatesProxy()
        {
            var container = BuildContainer();
            var service = container.Resolve<IService>();
            Assert.NotNull(service);
            Assert.IsAssignableFrom<IService>(service);
        }

        [Fact]
        public void Resolve_InterfaceService_ProxyIsNotConcreteType()
        {
            var container = BuildContainer();
            var service = container.Resolve<IService>();
            Assert.NotEqual(typeof(Service), service.GetType());
        }

        [Fact]
        public void Resolve_InterfaceService_CanInvokeMethod()
        {
            var container = BuildContainer();
            var service = container.Resolve<IService>();
            var result = service.Get(1);
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public void Resolve_ScopedService_DifferentPerScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();
            builder.RegisterType<Service>().As<IService>().InstancePerLifetimeScope();
            var container = builder.Build();

            IService s1;
            IService s2;
            using (var scope1 = container.BeginLifetimeScope())
            {
                s1 = scope1.Resolve<IService>();
            }
            using (var scope2 = container.BeginLifetimeScope())
            {
                s2 = scope2.Resolve<IService>();
            }
            Assert.NotSame(s1, s2);
        }

        [Fact]
        public void Resolve_SingletonService_SameInstance()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();
            builder.RegisterType<Service>().As<IService>().SingleInstance();
            var container = builder.Build();

            var s1 = container.Resolve<IService>();
            var s2 = container.Resolve<IService>();
            Assert.Same(s1, s2);
        }

        [Fact]
        public void Resolve_WithDelegateActivator_CreatesProxy()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();
            builder.Register(c => new Service()).As<IService>();
            var container = builder.Build();

            var service = container.Resolve<IService>();
            Assert.NotNull(service);
            Assert.IsAssignableFrom<IService>(service);
        }

        [Fact]
        public void Resolve_WithInstanceActivator_CreatesProxy()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();
            var instance = new Service();
            builder.RegisterInstance(instance).As<IService>();
            var container = builder.Build();

            var service = container.Resolve<IService>();
            Assert.NotNull(service);
            Assert.IsAssignableFrom<IService>(service);
        }

        [Fact]
        public void Resolve_ClassWithoutInterface_CreatesClassProxy()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();
            builder.RegisterType<ClassService>();
            var container = builder.Build();

            var service = container.Resolve<ClassService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void Resolve_ClassWithoutInterface_CanInvokeMethod()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();
            builder.RegisterType<ClassService>();
            var container = builder.Build();

            var service = container.Resolve<ClassService>();
            var result = service.Get(1);
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public void Resolve_GenericService_CreatesProxy()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();
            builder.RegisterGeneric(typeof(GenericServiceImpl<>)).As(typeof(IGenericService<>));
            var container = builder.Build();

            var service = container.Resolve<IGenericService<IService>>();
            Assert.NotNull(service);
        }

        [Fact]
        public void Resolve_MultipleServices_AllProxied()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();
            builder.RegisterType<Service>().As<IService>();
            builder.RegisterType<Service2>().As<IService2>();
            builder.RegisterType<Service3>().As<IService3>();
            var container = builder.Build();

            Assert.NotNull(container.Resolve<IService>());
            Assert.NotNull(container.Resolve<IService2>());
            Assert.NotNull(container.Resolve<IService3>());
        }

        [Fact]
        public void Resolve_SealedService_NotProxied()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();
            builder.RegisterType<SealedService>().As<ISealedService>();
            var container = builder.Build();

            var service = container.Resolve<ISealedService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void Resolve_ClassWithConstructorDependency_CreatesProxy()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();
            builder.RegisterType<Service>().As<IService>();
            builder.RegisterType<ClassWithDependency>();
            var container = builder.Build();

            var service = container.Resolve<ClassWithDependency>();
            Assert.NotNull(service);
            Assert.NotNull(service.GetService());
        }

        [Fact]
        public void Resolve_ClassWithMultipleConstructorParams_CreatesProxy()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();
            builder.RegisterType<Service>().As<IService>();
            builder.RegisterType<Service2>().As<IService2>();
            builder.RegisterType<ClassWithMultipleDependencies>();
            var container = builder.Build();

            var service = container.Resolve<ClassWithMultipleDependencies>();
            Assert.NotNull(service);
            Assert.NotNull(service.GetService());
            Assert.NotNull(service.GetService2());
        }

        [Fact]
        public void Resolve_ClassWithVirtualMethod_ProxiedCorrectly()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();
            builder.RegisterType<Service>().As<IService>();
            builder.RegisterType<ClassWithDependency>();
            var container = builder.Build();

            var service = container.Resolve<ClassWithDependency>();
            var result = service.Get(1);
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public void Resolve_ClassWithConstructorDependency_ScopedLifetime()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();
            builder.RegisterType<Service>().As<IService>().InstancePerLifetimeScope();
            builder.RegisterType<ClassWithDependency>().InstancePerLifetimeScope();
            var container = builder.Build();

            ClassWithDependency s1;
            ClassWithDependency s2;
            using (var scope1 = container.BeginLifetimeScope())
            {
                s1 = scope1.Resolve<ClassWithDependency>();
            }
            using (var scope2 = container.BeginLifetimeScope())
            {
                s2 = scope2.Resolve<ClassWithDependency>();
            }
            Assert.NotSame(s1, s2);
        }

        public class ClassService
        {
            public virtual Model Get(int id) => new Model { Id = id };
        }

        public class ClassWithDependency
        {
            private readonly IService _service;

            public ClassWithDependency(IService service)
            {
                _service = service;
            }

            public IService GetService() => _service;

            public virtual Model Get(int id) => _service.Get(id);
        }

        public class ClassWithMultipleDependencies
        {
            private readonly IService _service;
            private readonly IService2 _service2;

            public ClassWithMultipleDependencies(IService service, IService2 service2)
            {
                _service = service;
                _service2 = service2;
            }

            public IService GetService() => _service;
            public IService2 GetService2() => _service2;
        }

        public interface ISealedService { }
        public sealed class SealedService : ISealedService { }

        public interface IService2 { }
        public class Service2 : IService2 { }
        public interface IService3 { }
        public class Service3 : IService3 { }
        public interface IGenericService<T> { }
        public class GenericServiceImpl<T> : IGenericService<T> { }
    }
}
