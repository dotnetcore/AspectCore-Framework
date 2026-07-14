using System;
using System.Reflection;
using AspectCore.Configuration;
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
        [Fact]
        public void RegisterDynamicProxy_NullContainerBuilder_Throws()
        {
            ContainerBuilder builder = null;
            Assert.Throws<ArgumentNullException>(() => builder.RegisterDynamicProxy());
        }

        [Fact]
        public void RegisterDynamicProxy_WithConfiguration_NullContainerBuilder_Throws()
        {
            ContainerBuilder builder = null;
            Assert.Throws<ArgumentNullException>(() => builder.RegisterDynamicProxy(null, config => { }));
        }

        [Fact]
        public void Execute_ExceptsName_IsNotProxied()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate(next => ctx => next(ctx), Predicates.ForNameSpace("AspectCoreTest.Autofac"));
            });
            // Create a class whose name matches the excepts list (IHttpContextAccessor)
            builder.RegisterType<IHttpContextAccessor>().AsSelf();
            var container = builder.Build();
            var service = container.Resolve<IHttpContextAccessor>();
            // Should not be proxied because name matches excepts list
            Assert.False(service.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
        }

        [Fact]
        public void Execute_ClassProxy_WithResolutionParameters()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate(next => ctx => next(ctx), Predicates.ForNameSpace("AspectCoreTest.Autofac"));
            });
            builder.RegisterType<Service>().As<IService>();
            builder.RegisterType<ClassServiceWithDependency>().AsSelf();
            var container = builder.Build();
            // Resolve with explicit parameters to trigger EnumerateParameters path
            var service = container.Resolve<ClassServiceWithDependency>(
                new TypedParameter(typeof(IService), new Service()));
            Assert.NotNull(service);
            Assert.True(service.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
        }

        [Fact]
        public void Execute_ClassProxy_NoValidConstructors_Throws()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate(next => ctx => next(ctx), Predicates.ForNameSpace("AspectCoreTest.Autofac"));
            });
            // Register a class with a dependency that can't be resolved
            builder.RegisterType<ClassServiceWithUnresolvableDependency>().AsSelf();
            var container = builder.Build();
            // Resolving should throw because the dependency can't be found
            Assert.ThrowsAny<Exception>(() => container.Resolve<ClassServiceWithUnresolvableDependency>());
        }

        [Fact]
        public void Execute_ClassProxy_MultipleConstructors_TriggersFullParameterEnumeration()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate(next => ctx => next(ctx), Predicates.ForNameSpace("AspectCoreTest.Autofac"));
            });
            builder.RegisterType<Service>().As<IService>();
            // Class with two constructors: one resolvable, one not
            builder.RegisterType<ClassServiceWithTwoConstructors>().AsSelf();
            var container = builder.Build();
            // Resolve with explicit TypedParameter for the first constructor
            var service = container.Resolve<ClassServiceWithTwoConstructors>(
                new TypedParameter(typeof(IService), new Service()));
            Assert.NotNull(service);
            Assert.True(service.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
        }

        [Fact]
        public void Execute_AutoEngine_InterfaceProxy_FallsBackToDynamicProxy()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate(next => ctx => next(ctx), Predicates.ForNameSpace("AspectCore.Extensions.Test.Fakes"));
            });
            builder.ConfigureDynamicProxyEngine(options =>
            {
                options.Engine = ProxyEngine.Auto;
                options.AllowRuntimeFallback = true;
                options.Strict = false;
            });
            builder.RegisterType<Service>().As<IService>();
            var container = builder.Build();
            var service = container.Resolve<IService>();
            Assert.NotNull(service);
            Assert.True(service.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
        }
    }

    // Class whose name matches "IHttpContextAccessor" in the excepts list
    public class IHttpContextAccessor
    {
        public virtual string GetValue()
        {
            return "http";
        }
    }

    public class ClassServiceWithUnresolvableDependency
    {
        private readonly IUnregisteredDependency _dependency;

        public ClassServiceWithUnresolvableDependency(IUnregisteredDependency dependency)
        {
            _dependency = dependency;
        }

        public virtual string GetValue()
        {
            return _dependency?.ToString();
        }
    }

    public class ClassServiceWithTwoConstructors
    {
        private readonly IService _service;
        private readonly IUnregisteredDependency _dependency;

        public ClassServiceWithTwoConstructors(IService service)
        {
            _service = service;
        }

        public ClassServiceWithTwoConstructors(IUnregisteredDependency dependency)
        {
            _dependency = dependency;
        }

        public virtual string GetValue()
        {
            return _service?.Get(1)?.ToString() ?? _dependency?.ToString();
        }
    }

    public interface IUnregisteredDependency
    {
    }
}
