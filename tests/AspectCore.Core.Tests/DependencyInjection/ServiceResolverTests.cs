using System;
using System.Collections.Generic;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DependencyInjection
{
    public class ServiceResolverTests
    {
        [Fact]
        public void Resolve_WithNullServiceType_ThrowsArgumentNullException()
        {
            var context = new ServiceContext();
            var resolver = new ServiceResolver(context);
            var ex = Assert.Throws<ArgumentNullException>(() => resolver.Resolve(null));
            Assert.Equal("serviceType", ex.ParamName);
        }

        [Fact]
        public void Resolve_WithUnregisteredService_ReturnsNull()
        {
            var context = new ServiceContext();
            var resolver = new ServiceResolver(context);
            var result = resolver.Resolve(typeof(IFormattable));
            Assert.Null(result);
        }

        [Fact]
        public void Resolve_WithRegisteredService_ReturnsInstance()
        {
            var context = new ServiceContext();
            var instance = new DisposableImpl();
            context.Add(new InstanceServiceDefinition(typeof(IDisposable), instance));
            var resolver = new ServiceResolver(context);
            var result = resolver.Resolve(typeof(IDisposable));
            Assert.Same(instance, result);
        }

        [Fact]
        public void GetService_ReturnsSameAsResolve()
        {
            var context = new ServiceContext();
            var instance = new object();
            context.Add(new InstanceServiceDefinition(typeof(object), instance));
            var resolver = new ServiceResolver(context);
            var result = resolver.GetService(typeof(object));
            Assert.Same(instance, result);
        }

#if NET8_0_OR_GREATER
        [Fact]
        public void GetKeyedService_ReturnsService_ByType()
        {
            var context = new ServiceContext();
            var instance = new object();
            context.Add(new InstanceServiceDefinition(typeof(object), instance));
            var resolver = new ServiceResolver(context);
            var result = resolver.GetKeyedService(typeof(object), "key");
            Assert.NotNull(result);
            Assert.Same(instance, result);
        }

        [Fact]
        public void GetKeyedService_ReturnsNull_ForUnregisteredService()
        {
            var context = new ServiceContext();
            var resolver = new ServiceResolver(context);
            var result = resolver.GetKeyedService(typeof(object), "key");
            Assert.Null(result);
        }

        [Fact]
        public void GetRequiredKeyedService_ReturnsService_ByType()
        {
            var context = new ServiceContext();
            var instance = new object();
            context.Add(new InstanceServiceDefinition(typeof(object), instance));
            var resolver = new ServiceResolver(context);
            var result = resolver.GetRequiredKeyedService(typeof(object), "key");
            Assert.NotNull(result);
            Assert.Same(instance, result);
        }

        [Fact]
        public void GetRequiredKeyedService_Throws_ForUnregisteredService()
        {
            var context = new ServiceContext();
            var resolver = new ServiceResolver(context);
            Assert.Throws<InvalidOperationException>(() => resolver.GetRequiredKeyedService(typeof(object), "key"));
        }
#endif

        [Fact]
        public void Resolve_Singleton_ReturnsSameInstance()
        {
            var context = new ServiceContext();
            context.Add(new TypeServiceDefinition(typeof(IDisposable), typeof(DisposableImpl), Lifetime.Singleton));
            var resolver = new ServiceResolver(context);
            var result1 = resolver.Resolve(typeof(IDisposable));
            var result2 = resolver.Resolve(typeof(IDisposable));
            Assert.Same(result1, result2);
        }

        [Fact]
        public void Resolve_Transient_ReturnsDifferentInstances()
        {
            var context = new ServiceContext();
            context.Add(new TypeServiceDefinition(typeof(IDisposable), typeof(DisposableImpl), Lifetime.Transient));
            var resolver = new ServiceResolver(context);
            var result1 = resolver.Resolve(typeof(IDisposable));
            var result2 = resolver.Resolve(typeof(IDisposable));
            Assert.NotSame(result1, result2);
        }

        [Fact]
        public void Resolve_Scoped_ReturnsSameInstanceInSameScope()
        {
            var context = new ServiceContext();
            context.Add(new TypeServiceDefinition(typeof(IDisposable), typeof(DisposableImpl), Lifetime.Scoped));
            var resolver = new ServiceResolver(context);
            var result1 = resolver.Resolve(typeof(IDisposable));
            var result2 = resolver.Resolve(typeof(IDisposable));
            Assert.Same(result1, result2);
        }

        [Fact]
        public void Dispose_DisposesResolvedScopedServices()
        {
            var context = new ServiceContext();
            context.Add(new TypeServiceDefinition(typeof(IDisposable), typeof(DisposableImpl), Lifetime.Scoped));
            var resolver = new ServiceResolver(context);
            var result = resolver.Resolve(typeof(IDisposable));
            var disposable = Assert.IsType<DisposableImpl>(result);
            resolver.Dispose();
            Assert.True(disposable.Disposed);
        }

        [Fact]
        public void Dispose_DisposesResolvedSingletonServices()
        {
            var context = new ServiceContext();
            context.Add(new TypeServiceDefinition(typeof(IDisposable), typeof(DisposableImpl), Lifetime.Singleton));
            var resolver = new ServiceResolver(context);
            var result = resolver.Resolve(typeof(IDisposable));
            var disposable = Assert.IsType<DisposableImpl>(result);
            resolver.Dispose();
            Assert.True(disposable.Disposed);
        }

        [Fact]
        public void Dispose_CanBeCalledMultipleTimes()
        {
            var context = new ServiceContext();
            var resolver = new ServiceResolver(context);
            resolver.Dispose();
            resolver.Dispose();
        }

        [Fact]
        public void Constructor_WithRootResolver_SharedSingletonServices()
        {
            var context = new ServiceContext();
            context.Add(new TypeServiceDefinition(typeof(IDisposable), typeof(DisposableImpl), Lifetime.Singleton));
            var rootResolver = new ServiceResolver(context);
            var childResolver = new ServiceResolver(rootResolver);

            var rootResult = rootResolver.Resolve(typeof(IDisposable));
            var childResult = childResolver.Resolve(typeof(IDisposable));
            Assert.Same(rootResult, childResult);
        }

        private class DisposableImpl : IDisposable
        {
            public bool Disposed { get; private set; }
            public void Dispose() => Disposed = true;
        }
    }
}
