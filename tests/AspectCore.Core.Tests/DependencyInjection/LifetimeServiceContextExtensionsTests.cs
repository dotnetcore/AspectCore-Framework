using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AspectCore.DependencyInjection;
using Xunit;

namespace AspectCore.Core.Tests.DependencyInjection
{
    public class LifetimeServiceContextExtensionsTests
    {
        #region AddType

        [Fact]
        public void AddType_WithServiceTypeOnly_AddsTypeServiceDefinitionWithContextLifetime()
        {
            var context = new FakeLifetimeServiceContext(Lifetime.Singleton);
            var result = context.AddType(typeof(IDisposable));
            Assert.Same(context, result);
            Assert.Single(context.Items);
            var def = Assert.IsType<TypeServiceDefinition>(context.Items[0]);
            Assert.Equal(typeof(IDisposable), def.ServiceType);
            Assert.Equal(typeof(IDisposable), def.ImplementationType);
            Assert.Equal(Lifetime.Singleton, def.Lifetime);
        }

        [Fact]
        public void AddType_WithNullLifetimeServiceContext_ThrowsArgumentNullException()
        {
            ILifetimeServiceContext context = null;
            var ex = Assert.Throws<ArgumentNullException>(() => context.AddType(typeof(IDisposable), typeof(string)));
            Assert.Equal("lifetimeServiceContext", ex.ParamName);
        }

        [Fact]
        public void AddType_WithServiceTypeAndImplementationType_AddsTypeServiceDefinition()
        {
            var context = new FakeLifetimeServiceContext(Lifetime.Scoped);
            var result = context.AddType(typeof(IDisposable), typeof(string));
            Assert.Same(context, result);
            Assert.Single(context.Items);
            var def = Assert.IsType<TypeServiceDefinition>(context.Items[0]);
            Assert.Equal(typeof(IDisposable), def.ServiceType);
            Assert.Equal(typeof(string), def.ImplementationType);
            Assert.Equal(Lifetime.Scoped, def.Lifetime);
        }

        [Fact]
        public void AddType_Generic_AddsTypeServiceDefinitionWithContextLifetime()
        {
            var context = new FakeLifetimeServiceContext(Lifetime.Transient);
            var result = context.AddType<IDisposable>();
            Assert.Same(context, result);
            Assert.Single(context.Items);
            var def = Assert.IsType<TypeServiceDefinition>(context.Items[0]);
            Assert.Equal(typeof(IDisposable), def.ServiceType);
            Assert.Equal(typeof(IDisposable), def.ImplementationType);
            Assert.Equal(Lifetime.Transient, def.Lifetime);
        }

        [Fact]
        public void AddType_GenericWithImplementation_WithNullContext_ThrowsArgumentNullException()
        {
            ILifetimeServiceContext context = null;
            var ex = Assert.Throws<ArgumentNullException>(() => context.AddType<IComparable, string>());
            Assert.Equal("lifetimeServiceContext", ex.ParamName);
        }

        [Fact]
        public void AddType_GenericWithImplementation_AddsTypeServiceDefinition()
        {
            var context = new FakeLifetimeServiceContext(Lifetime.Singleton);
            var result = context.AddType<IComparable, string>();
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
            var context = new FakeLifetimeServiceContext(Lifetime.Singleton);
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
            var context = new FakeLifetimeServiceContext(Lifetime.Singleton);
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
        public void AddDelegate_WithTypeFunc_AddsDelegateServiceDefinitionWithContextLifetime()
        {
            var context = new FakeLifetimeServiceContext(Lifetime.Scoped);
            Func<IServiceResolver, object> impl = r => new object();
            var result = context.AddDelegate(typeof(object), impl);
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
            var context = new FakeLifetimeServiceContext(Lifetime.Transient);
            Func<IServiceResolver, string> impl = r => "test";
            var result = context.AddDelegate<object, string>(impl);
            Assert.Same(context, result);
            Assert.Single(context.Items);
            var def = Assert.IsType<DelegateServiceDefinition>(context.Items[0]);
            Assert.Equal(typeof(object), def.ServiceType);
            Assert.Same(impl, def.ImplementationDelegate);
            Assert.Equal(Lifetime.Transient, def.Lifetime);
        }

        [Fact]
        public void AddDelegate_GenericSingle_AddsDelegateServiceDefinition()
        {
            var context = new FakeLifetimeServiceContext(Lifetime.Singleton);
            Func<IServiceResolver, string> impl = r => "test";
            var result = context.AddDelegate<string>(impl);
            Assert.Same(context, result);
            Assert.Single(context.Items);
            var def = Assert.IsType<DelegateServiceDefinition>(context.Items[0]);
            Assert.Equal(typeof(string), def.ServiceType);
            Assert.Same(impl, def.ImplementationDelegate);
            Assert.Equal(Lifetime.Singleton, def.Lifetime);
        }

        #endregion

        #region Test Types

        private class FakeLifetimeServiceContext : ILifetimeServiceContext
        {
            public List<ServiceDefinition> Items { get; } = new List<ServiceDefinition>();

            public Lifetime Lifetime { get; }

            public int Count => Items.Count;

            public FakeLifetimeServiceContext(Lifetime lifetime) => Lifetime = lifetime;

            public void Add(ServiceDefinition item) => Items.Add(item);

            public bool Contains(Type serviceType) => Items.Any(d => d.ServiceType == serviceType);

            public IEnumerator<ServiceDefinition> GetEnumerator() => Items.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        #endregion
    }
}
