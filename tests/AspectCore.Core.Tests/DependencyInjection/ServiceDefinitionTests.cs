using System;
using AspectCore.DependencyInjection;
using Xunit;

namespace AspectCore.Core.Tests.DependencyInjection
{
    public class ServiceDefinitionTests
    {
        #region ServiceDefinition (base)

        [Fact]
        public void Constructor_WithNullServiceType_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new TypeServiceDefinition(null, typeof(string), Lifetime.Singleton));
            Assert.Equal("serviceType", ex.ParamName);
        }

        [Fact]
        public void Constructor_WithValidArguments_StoresServiceType()
        {
            var definition = new TypeServiceDefinition(typeof(IDisposable), typeof(string), Lifetime.Singleton);
            Assert.Equal(typeof(IDisposable), definition.ServiceType);
        }

        [Fact]
        public void Constructor_WithValidArguments_StoresLifetime()
        {
            var definition = new TypeServiceDefinition(typeof(IDisposable), typeof(string), Lifetime.Scoped);
            Assert.Equal(Lifetime.Scoped, definition.Lifetime);
        }

        #endregion

        #region TypeServiceDefinition

        [Fact]
        public void TypeServiceDefinition_Constructor_WithNullImplementationType_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new TypeServiceDefinition(typeof(IDisposable), null, Lifetime.Singleton));
            Assert.Equal("implementationType", ex.ParamName);
        }

        [Fact]
        public void TypeServiceDefinition_Constructor_WithValidArguments_StoresImplementationType()
        {
            var definition = new TypeServiceDefinition(typeof(IDisposable), typeof(string), Lifetime.Transient);
            Assert.Equal(typeof(string), definition.ImplementationType);
        }

        [Fact]
        public void TypeServiceDefinition_IsSealed()
        {
            Assert.True(typeof(TypeServiceDefinition).IsSealed);
        }

        #endregion

        #region InstanceServiceDefinition

        [Fact]
        public void InstanceServiceDefinition_Constructor_WithNullImplementationInstance_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new InstanceServiceDefinition(typeof(IDisposable), null));
            Assert.Equal("implementationInstance", ex.ParamName);
        }

        [Fact]
        public void InstanceServiceDefinition_Constructor_WithValidArguments_StoresImplementationInstance()
        {
            var instance = new object();
            var definition = new InstanceServiceDefinition(typeof(object), instance);
            Assert.Same(instance, definition.ImplementationInstance);
        }

        [Fact]
        public void InstanceServiceDefinition_Constructor_AlwaysUsesSingletonLifetime()
        {
            var definition = new InstanceServiceDefinition(typeof(object), new object());
            Assert.Equal(Lifetime.Singleton, definition.Lifetime);
        }

        [Fact]
        public void InstanceServiceDefinition_IsSealed()
        {
            Assert.True(typeof(InstanceServiceDefinition).IsSealed);
        }

        #endregion

        #region DelegateServiceDefinition

        [Fact]
        public void DelegateServiceDefinition_Constructor_WithNullImplementationDelegate_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new DelegateServiceDefinition(typeof(IDisposable), null, Lifetime.Singleton));
            Assert.Equal("implementationDelegate", ex.ParamName);
        }

        [Fact]
        public void DelegateServiceDefinition_Constructor_WithValidArguments_StoresImplementationDelegate()
        {
            Func<IServiceResolver, object> implementationDelegate = resolver => new object();
            var definition = new DelegateServiceDefinition(typeof(object), implementationDelegate, Lifetime.Scoped);
            Assert.Same(implementationDelegate, definition.ImplementationDelegate);
        }

        [Fact]
        public void DelegateServiceDefinition_Constructor_WithValidArguments_StoresLifetime()
        {
            Func<IServiceResolver, object> implementationDelegate = resolver => new object();
            var definition = new DelegateServiceDefinition(typeof(object), implementationDelegate, Lifetime.Transient);
            Assert.Equal(Lifetime.Transient, definition.Lifetime);
        }

        [Fact]
        public void DelegateServiceDefinition_ImplementationDelegate_IsInvokable()
        {
            var expected = new object();
            Func<IServiceResolver, object> implementationDelegate = resolver => expected;
            var definition = new DelegateServiceDefinition(typeof(object), implementationDelegate, Lifetime.Singleton);
            var result = definition.ImplementationDelegate(null);
            Assert.Same(expected, result);
        }

        #endregion
    }
}
