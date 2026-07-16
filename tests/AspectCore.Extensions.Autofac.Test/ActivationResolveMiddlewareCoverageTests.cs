using System;
using System.Reflection;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Autofac;
using AspectCore.Extensions.Test.Fakes;
using Autofac;
using Autofac.Core;
using Xunit;

namespace AspectCoreTest.Autofac
{
    public class ActivationResolveMiddlewareCoverageTests
    {
        [Fact]
        public void Execute_ClassProxyWithParameters_EnumeratesParameters()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate(next => ctx => next(ctx), Predicates.ForNameSpace("AspectCoreTest.Autofac"));
            });
            builder.RegisterType<ClassServiceWithParam>().AsSelf();
            var container = builder.Build();
            // Resolve with parameters to trigger EnumerateParameters in GetAllBindings
            var param = new NamedParameter("name", "test-param");
            var service = container.Resolve<ClassServiceWithParam>(param);
            Assert.NotNull(service);
            Assert.Equal("test-param", service.Name);
        }

        [Fact]
        public void Execute_ClassProxyWithTypedParameter_EnumeratesParameters()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate(next => ctx => next(ctx), Predicates.ForNameSpace("AspectCoreTest.Autofac"));
            });
            builder.RegisterType<ClassServiceWithParam>().AsSelf();
            var container = builder.Build();
            // Resolve with typed parameter to trigger EnumerateParameters
            var param = new TypedParameter(typeof(string), "typed-param");
            var service = container.Resolve<ClassServiceWithParam>(param);
            Assert.NotNull(service);
            Assert.Equal("typed-param", service.Name);
        }

        [Fact]
        public void Execute_ClassProxyWithMultipleParameters_EnumeratesParameters()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate(next => ctx => next(ctx), Predicates.ForNameSpace("AspectCoreTest.Autofac"));
            });
            builder.RegisterType<ClassServiceWithMultipleParams>().AsSelf();
            var container = builder.Build();
            // Resolve with multiple parameters
            var param1 = new NamedParameter("name", "multi-param");
            var param2 = new NamedParameter("count", 42);
            var service = container.Resolve<ClassServiceWithMultipleParams>(param1, param2);
            Assert.NotNull(service);
            Assert.Equal("multi-param", service.Name);
            Assert.Equal(42, service.Count);
        }
    }

    // Class service with constructor parameter for EnumerateParameters test
    public class ClassServiceWithParam
    {
        public string Name { get; }

        public ClassServiceWithParam(string name)
        {
            Name = name;
        }

        public virtual string GetValue() => Name;
    }

    public class ClassServiceWithMultipleParams
    {
        public string Name { get; }
        public int Count { get; }

        public ClassServiceWithMultipleParams(string name, int count)
        {
            Name = name;
            Count = count;
        }

        public virtual string GetValue() => $"{Name}-{Count}";
    }
}
