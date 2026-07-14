using System;
using System.Collections.Generic;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DependencyInjection
{
    public class ConstructorCallSiteResolverTests
    {
        private static ConstructorCallSiteResolver CreateResolver(IEnumerable<ServiceDefinition> services = null)
        {
            var context = new ServiceContext(services ?? new List<ServiceDefinition>());
            var table = new ServiceTable(context);
            table.Populate(context);
            return new ConstructorCallSiteResolver(table);
        }

        [Fact]
        public void Resolve_WithParameterlessConstructor_ReturnsCallSite()
        {
            var resolver = CreateResolver();
            var callSite = resolver.Resolve(typeof(ParameterlessClass));
            Assert.NotNull(callSite);
        }

        [Fact]
        public void Resolve_WithParameterlessConstructor_InvokesConstructor()
        {
            var resolver = CreateResolver();
            var callSite = resolver.Resolve(typeof(ParameterlessClass));
            var serviceResolver = CreateServiceResolver();
            var result = callSite(serviceResolver);
            Assert.NotNull(result);
            Assert.IsType<ParameterlessClass>(result);
        }

        [Fact]
        public void Resolve_WithConstructorThatHasResolvableParameters_ReturnsCallSite()
        {
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IDependency), typeof(DependencyImpl), Lifetime.Transient)
            };
            var resolver = CreateResolver(services);
            var callSite = resolver.Resolve(typeof(WithDependencyClass));
            Assert.NotNull(callSite);
        }

        [Fact]
        public void Resolve_WithConstructorThatHasResolvableParameters_InvokesConstructor()
        {
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IDependency), typeof(DependencyImpl), Lifetime.Transient)
            };
            var resolver = CreateResolver(services);
            var callSite = resolver.Resolve(typeof(WithDependencyClass));
            var serviceResolver = CreateServiceResolver(services);
            var result = callSite(serviceResolver);
            Assert.NotNull(result);
            Assert.IsType<WithDependencyClass>(result);
        }

        [Fact]
        public void Resolve_WithMultipleConstructors_PicksOneWithMostResolvableParameters()
        {
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IDependency), typeof(DependencyImpl), Lifetime.Transient)
            };
            var resolver = CreateResolver(services);
            var callSite = resolver.Resolve(typeof(MultipleConstructorsClass));
            Assert.NotNull(callSite);
        }

        [Fact]
        public void Resolve_WithConstructorThatHasDefaultValueParameter_ReturnsCallSite()
        {
            var resolver = CreateResolver();
            var callSite = resolver.Resolve(typeof(WithDefaultValueClass));
            Assert.NotNull(callSite);
        }

        [Fact]
        public void Resolve_WithConstructorThatHasDefaultValueParameter_InvokesConstructor()
        {
            var resolver = CreateResolver();
            var callSite = resolver.Resolve(typeof(WithDefaultValueClass));
            var serviceResolver = CreateServiceResolver();
            var result = callSite(serviceResolver);
            Assert.NotNull(result);
            Assert.IsType<WithDefaultValueClass>(result);
        }

        [Fact]
        public void Resolve_WithNoPublicConstructor_ReturnsNull()
        {
            var resolver = CreateResolver();
            var callSite = resolver.Resolve(typeof(NoPublicConstructorClass));
            Assert.Null(callSite);
        }

        [Fact]
        public void Resolve_WithUnresolvableParameter_ReturnsNull()
        {
            var resolver = CreateResolver();
            var callSite = resolver.Resolve(typeof(WithUnresolvableDependencyClass));
            Assert.Null(callSite);
        }

        [Fact]
        public void Resolve_CachesResultForSameType()
        {
            var resolver = CreateResolver();
            var callSite1 = resolver.Resolve(typeof(ParameterlessClass));
            var callSite2 = resolver.Resolve(typeof(ParameterlessClass));
            Assert.Same(callSite1, callSite2);
        }

        private static IServiceResolver CreateServiceResolver(IEnumerable<ServiceDefinition> services = null)
        {
            var context = new ServiceContext(services ?? new List<ServiceDefinition>());
            return new ServiceResolver(context);
        }

        private class ParameterlessClass { }

        private class NoPublicConstructorClass
        {
            private NoPublicConstructorClass() { }
        }

        private interface IDependency { }

        private class DependencyImpl : IDependency { }

        private class WithDependencyClass
        {
            public IDependency Dependency { get; }
            public WithDependencyClass(IDependency dependency)
            {
                Dependency = dependency;
            }
        }

        private class WithUnresolvableDependencyClass
        {
            public WithUnresolvableDependencyClass(IUnknownDependency dependency) { }
        }

        private interface IUnknownDependency { }

        private class MultipleConstructorsClass
        {
            public IDependency Dependency { get; }
            public string Name { get; }

            public MultipleConstructorsClass()
            {
            }

            public MultipleConstructorsClass(IDependency dependency)
            {
                Dependency = dependency;
            }

            public MultipleConstructorsClass(IDependency dependency, string name)
            {
                Dependency = dependency;
                Name = name;
            }
        }

        private class WithDefaultValueClass
        {
            public string Name { get; }
            public WithDefaultValueClass(string name = "default")
            {
                Name = name;
            }
        }
    }
}
