using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DependencyInjection
{
    public class ServiceContextExtensionsTests
    {
        #region AddType

        [Fact]
        public void AddType_WithNullServiceContext_ThrowsArgumentNullException()
        {
            IServiceContext serviceContext = null;
            var ex = Assert.Throws<ArgumentNullException>(() => serviceContext.AddType(typeof(IDisposable)));
            Assert.Equal("serviceContext", ex.ParamName);
        }

        [Fact]
        public void AddType_WithServiceTypeOnly_AddsTypeServiceDefinition()
        {
            var context = new FakeServiceContext();
            var result = context.AddType(typeof(IDisposable));
            Assert.Same(context, result);
            Assert.Single(context.Items);
            var def = Assert.IsType<TypeServiceDefinition>(context.Items[0]);
            Assert.Equal(typeof(IDisposable), def.ServiceType);
            Assert.Equal(typeof(IDisposable), def.ImplementationType);
            Assert.Equal(Lifetime.Transient, def.Lifetime);
        }

        [Fact]
        public void AddType_WithServiceTypeAndImplementationType_AddsTypeServiceDefinition()
        {
            var context = new FakeServiceContext();
            var result = context.AddType(typeof(IDisposable), typeof(string), Lifetime.Singleton);
            Assert.Same(context, result);
            Assert.Single(context.Items);
            var def = Assert.IsType<TypeServiceDefinition>(context.Items[0]);
            Assert.Equal(typeof(IDisposable), def.ServiceType);
            Assert.Equal(typeof(string), def.ImplementationType);
            Assert.Equal(Lifetime.Singleton, def.Lifetime);
        }

        [Fact]
        public void AddType_Generic_AddsTypeServiceDefinition()
        {
            var context = new FakeServiceContext();
            var result = context.AddType<IDisposable>(Lifetime.Scoped);
            Assert.Same(context, result);
            Assert.Single(context.Items);
            var def = Assert.IsType<TypeServiceDefinition>(context.Items[0]);
            Assert.Equal(typeof(IDisposable), def.ServiceType);
            Assert.Equal(Lifetime.Scoped, def.Lifetime);
        }

        [Fact]
        public void AddType_GenericWithImplementation_AddsTypeServiceDefinition()
        {
            var context = new FakeServiceContext();
            var result = context.AddType<IComparable, string>(Lifetime.Singleton);
            Assert.Same(context, result);
            Assert.Single(context.Items);
            var def = Assert.IsType<TypeServiceDefinition>(context.Items[0]);
            Assert.Equal(typeof(IComparable), def.ServiceType);
            Assert.Equal(typeof(string), def.ImplementationType);
            Assert.Equal(Lifetime.Singleton, def.Lifetime);
        }

        #endregion

        #region AddInstance

        [Fact]
        public void AddInstance_WithTypeAndInstance_AddsInstanceServiceDefinition()
        {
            var context = new FakeServiceContext();
            var instance = new object();
            var result = context.AddInstance(typeof(object), instance);
            Assert.Same(context, result);
            Assert.Single(context.Items);
            var def = Assert.IsType<InstanceServiceDefinition>(context.Items[0]);
            Assert.Equal(typeof(object), def.ServiceType);
            Assert.Same(instance, def.ImplementationInstance);
        }

        [Fact]
        public void AddInstance_Generic_AddsInstanceServiceDefinition()
        {
            var context = new FakeServiceContext();
            var instance = "test";
            var result = context.AddInstance<string>(instance);
            Assert.Same(context, result);
            Assert.Single(context.Items);
            var def = Assert.IsType<InstanceServiceDefinition>(context.Items[0]);
            Assert.Equal(typeof(string), def.ServiceType);
            Assert.Same(instance, def.ImplementationInstance);
        }

        #endregion

        #region AddDelegate

        [Fact]
        public void AddDelegate_WithTypeFuncLifetime_AddsDelegateServiceDefinition()
        {
            var context = new FakeServiceContext();
            Func<IServiceResolver, object> impl = r => new object();
            var result = context.AddDelegate(typeof(object), impl, Lifetime.Scoped);
            Assert.Same(context, result);
            Assert.Single(context.Items);
            var def = Assert.IsType<DelegateServiceDefinition>(context.Items[0]);
            Assert.Equal(typeof(object), def.ServiceType);
            Assert.Same(impl, def.ImplementationDelegate);
            Assert.Equal(Lifetime.Scoped, def.Lifetime);
        }

        [Fact]
        public void AddDelegate_GenericWithImplementation_AddsDelegateServiceDefinition()
        {
            var context = new FakeServiceContext();
            Func<IServiceResolver, string> impl = r => "test";
            var result = context.AddDelegate<object, string>(impl, Lifetime.Singleton);
            Assert.Same(context, result);
            Assert.Single(context.Items);
            var def = Assert.IsType<DelegateServiceDefinition>(context.Items[0]);
            Assert.Equal(typeof(object), def.ServiceType);
            Assert.Same(impl, def.ImplementationDelegate);
            Assert.Equal(Lifetime.Singleton, def.Lifetime);
        }

        [Fact]
        public void AddDelegate_GenericSingle_AddsDelegateServiceDefinition()
        {
            var context = new FakeServiceContext();
            Func<IServiceResolver, string> impl = r => "test";
            var result = context.AddDelegate<string>(impl, Lifetime.Transient);
            Assert.Same(context, result);
            Assert.Single(context.Items);
            var def = Assert.IsType<DelegateServiceDefinition>(context.Items[0]);
            Assert.Equal(typeof(string), def.ServiceType);
            Assert.Same(impl, def.ImplementationDelegate);
            Assert.Equal(Lifetime.Transient, def.Lifetime);
        }

        #endregion

        #region RemoveAll

        [Fact]
        public void RemoveAll_Generic_RemovesAllMatchingDefinitions()
        {
            var context = new FakeServiceContext();
            context.AddType<IDisposable>();
            context.AddType<IDisposable>(Lifetime.Singleton);
            context.AddType<IComparable>();
            Assert.Equal(3, context.Items.Count);

            var result = context.RemoveAll<IDisposable>();
            Assert.Same(context, result);
            Assert.Single(context.Items);
            Assert.Equal(typeof(IComparable), context.Items[0].ServiceType);
        }

        [Fact]
        public void RemoveAll_WithType_RemovesAllMatchingDefinitions()
        {
            var context = new FakeServiceContext();
            context.AddType<IDisposable>();
            context.AddType<IComparable>();
            context.AddType<IDisposable>(Lifetime.Scoped);

            var result = context.RemoveAll(typeof(IDisposable));
            Assert.Same(context, result);
            Assert.Single(context.Items);
            Assert.Equal(typeof(IComparable), context.Items[0].ServiceType);
        }

        [Fact]
        public void RemoveAll_WhenNoMatches_DoesNotRemoveAnything()
        {
            var context = new FakeServiceContext();
            context.AddType<IDisposable>();
            var result = context.RemoveAll(typeof(IComparable));
            Assert.Same(context, result);
            Assert.Single(context.Items);
        }

        #endregion

        #region ConfigureDynamicProxyEngine

        [Fact]
        public void ConfigureDynamicProxyEngine_WithNullServiceContext_ThrowsArgumentNullException()
        {
            IServiceContext serviceContext = null;
            var ex = Assert.Throws<ArgumentNullException>(() => serviceContext.ConfigureDynamicProxyEngine(o => { }));
            Assert.Equal("serviceContext", ex.ParamName);
        }

        [Fact]
        public void ConfigureDynamicProxyEngine_WithNoExisting_AddsProxyEngineOptions()
        {
            var context = new FakeServiceContext();
            var result = context.ConfigureDynamicProxyEngine(o =>
            {
                o.Engine = ProxyEngine.SourceGenerator;
                o.Strict = true;
            });
            Assert.Same(context, result);
            var def = Assert.IsType<InstanceServiceDefinition>(context.Items[0]);
            Assert.Equal(typeof(ProxyEngineOptions), def.ServiceType);
            var options = Assert.IsType<ProxyEngineOptions>(def.ImplementationInstance);
            Assert.Equal(ProxyEngine.SourceGenerator, options.Engine);
            Assert.True(options.Strict);
        }

        [Fact]
        public void ConfigureDynamicProxyEngine_WithExisting_ReplacesExistingOptions()
        {
            var context = new FakeServiceContext();
            context.ConfigureDynamicProxyEngine(o => o.Engine = ProxyEngine.DynamicProxy);
            Assert.Single(context.Items);

            var result = context.ConfigureDynamicProxyEngine(o => o.Engine = ProxyEngine.SourceGenerator);
            Assert.Same(context, result);
            Assert.Single(context.Items);
            var def = Assert.IsType<InstanceServiceDefinition>(context.Items[0]);
            var options = Assert.IsType<ProxyEngineOptions>(def.ImplementationInstance);
            Assert.Equal(ProxyEngine.SourceGenerator, options.Engine);
        }

        [Fact]
        public void ConfigureDynamicProxyEngine_WithNullConfigure_UsesDefaultOptions()
        {
            var context = new FakeServiceContext();
            var result = context.ConfigureDynamicProxyEngine(null);
            Assert.Same(context, result);
            Assert.Single(context.Items);
            var def = Assert.IsType<InstanceServiceDefinition>(context.Items[0]);
            var options = Assert.IsType<ProxyEngineOptions>(def.ImplementationInstance);
            Assert.Equal(ProxyEngine.DynamicProxy, options.Engine);
        }

        #endregion

        #region AddSourceGeneratedProxyRegistry

        [Fact]
        public void AddSourceGeneratedProxyRegistry_WithNullServiceContext_ThrowsArgumentNullException()
        {
            IServiceContext serviceContext = null;
            var ex = Assert.Throws<ArgumentNullException>(() => serviceContext.AddSourceGeneratedProxyRegistry(new FakeRegistry()));
            Assert.Equal("serviceContext", ex.ParamName);
        }

        [Fact]
        public void AddSourceGeneratedProxyRegistry_WithNullRegistry_ThrowsArgumentNullException()
        {
            var context = new FakeServiceContext();
            var ex = Assert.Throws<ArgumentNullException>(() => context.AddSourceGeneratedProxyRegistry(null));
            Assert.Equal("registry", ex.ParamName);
        }

        [Fact]
        public void AddSourceGeneratedProxyRegistry_WithValidRegistry_AddsInstanceDefinition()
        {
            var context = new FakeServiceContext();
            var registry = new FakeRegistry();
            var result = context.AddSourceGeneratedProxyRegistry(registry);
            Assert.Same(context, result);
            Assert.Single(context.Items);
            var def = Assert.IsType<InstanceServiceDefinition>(context.Items[0]);
            Assert.Equal(typeof(ISourceGeneratedProxyRegistry), def.ServiceType);
            Assert.Same(registry, def.ImplementationInstance);
        }

        #endregion

        #region Test Types

        private class FakeRegistry : ISourceGeneratedProxyRegistry
        {
            public bool TryGetProxyType(Type serviceType, Type implementationType, SourceGeneratedProxyKind kind, out Type proxyType)
            {
                proxyType = null;
                return false;
            }
        }

        private class FakeServiceContext : IServiceContext
        {
            public List<ServiceDefinition> Items { get; } = new List<ServiceDefinition>();

            public IAspectConfiguration Configuration { get; } = new AspectConfiguration();

            public ILifetimeServiceContext Singletons => null;

            public ILifetimeServiceContext Scopeds => null;

            public ILifetimeServiceContext Transients => null;

            public int Count => Items.Count;

            public void Add(ServiceDefinition item) => Items.Add(item);

            public bool Remove(ServiceDefinition item) => Items.Remove(item);

            public bool Contains(Type serviceType) => Items.Any(d => d.ServiceType == serviceType);

            public IEnumerator<ServiceDefinition> GetEnumerator() => Items.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        #endregion
    }
}
