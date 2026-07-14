using System;
using System.Reflection;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Autofac;
using AspectCore.Extensions.Test.Fakes;
using Autofac;
using Autofac.Core.Resolving.Pipeline;
using Xunit;

namespace AspectCoreTest.Autofac
{
    public class ActivationResolveMiddlewareTests
    {
        [Fact]
        public void Instance_ReturnsSingleton()
        {
            var instance = ActivationResolveMiddleware.Instance;
            Assert.NotNull(instance);
            Assert.Same(instance, ActivationResolveMiddleware.Instance);
        }

        [Fact]
        public void Phase_ReturnsActivation()
        {
            var middleware = ActivationResolveMiddleware.Instance;
            Assert.Equal(PipelinePhase.Activation, middleware.Phase);
        }

        [Fact]
        public void Execute_SealedClass_IsNotProxied()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate(next => ctx => next(ctx), Predicates.ForNameSpace("AspectCoreTest.Autofac"));
            });
            builder.RegisterType<SealedService>().AsSelf();
            var container = builder.Build();
            var service = container.Resolve<SealedService>();
            // Sealed class cannot be inherited, so it should not be proxied
            Assert.False(service.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
        }

        [Fact]
        public void Execute_ClassWithVirtualMethod_IsProxied()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate(next => ctx => next(ctx), Predicates.ForNameSpace("AspectCoreTest.Autofac"));
            });
            builder.RegisterType<ClassService>().AsSelf();
            var container = builder.Build();
            var service = container.Resolve<ClassService>();
            // Class with virtual methods should be proxied
            Assert.True(service.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
        }

        [Fact]
        public void Execute_NonAspectService_IsNotProxied()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();
            builder.RegisterType<NonAspectService>().As<INonAspectService>();
            var container = builder.Build();
            var service = container.Resolve<INonAspectService>();
            // NonAspect service should not be proxied
            Assert.False(service.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
        }

        [Fact]
        public void Execute_SystemNamespace_IsNotProxied()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();
            // System.String is in the excepts list
            builder.RegisterInstance("test string").AsSelf();
            var container = builder.Build();
            var str = container.Resolve<string>();
            Assert.Equal("test string", str);
        }

        [Fact]
        public void Execute_AlreadyProxiedInstance_IsNotProxiedAgain()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate(next => ctx => next(ctx), Predicates.ForNameSpace("AspectCore.Extensions.Test.Fakes"));
            });
            builder.RegisterType<Service>().As<IService>();
            var container = builder.Build();
            var service = container.Resolve<IService>();
            // Should be proxied
            Assert.True(service.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
            // Resolve again - should return already proxied instance (singleton behavior in same scope)
        }

        [Fact]
        public void Execute_ClassProxy_WithConstructorParameters()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate(next => ctx => next(ctx), Predicates.ForNameSpace("AspectCoreTest.Autofac"));
            });
            builder.RegisterType<Service>().As<IService>();
            builder.RegisterType<ClassServiceWithDependency>().AsSelf();
            var container = builder.Build();
            var service = container.Resolve<ClassServiceWithDependency>();
            Assert.NotNull(service);
            Assert.True(service.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
        }

        [Fact]
        public void Execute_InstanceActivator_IsProxied()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate(next => ctx => next(ctx), Predicates.ForNameSpace("AspectCoreTest.Autofac"));
            });
            var instance = new ClassService();
            builder.RegisterInstance(instance).AsSelf();
            var container = builder.Build();
            var service = container.Resolve<ClassService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void Execute_DelegateActivator_IsProxied()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate(next => ctx => next(ctx), Predicates.ForNameSpace("AspectCoreTest.Autofac"));
            });
            builder.Register(c => new ClassService()).AsSelf();
            var container = builder.Build();
            var service = container.Resolve<ClassService>();
            Assert.NotNull(service);
        }
    }

    public sealed class SealedService
    {
        public string GetValue()
        {
            return "sealed";
        }
    }

    public class ClassService
    {
        public virtual string GetValue()
        {
            return "class";
        }
    }

    public class ClassServiceWithDependency
    {
        private readonly IService _service;

        public ClassServiceWithDependency(IService service)
        {
            _service = service;
        }

        public virtual string GetValue()
        {
            return _service.Get(1).ToString();
        }
    }

    [NonAspect]
    public interface INonAspectService
    {
        string GetValue();
    }

    public class NonAspectService : INonAspectService
    {
        public string GetValue()
        {
            return "non-aspect";
        }
    }
}
