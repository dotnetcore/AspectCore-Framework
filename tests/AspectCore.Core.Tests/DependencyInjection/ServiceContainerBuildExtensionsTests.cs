using System;
using AspectCore.DependencyInjection;
using Xunit;

namespace AspectCore.Core.Tests.DependencyInjection
{
    public class ServiceContainerBuildExtensionsTests
    {
        [Fact]
        public void Build_WithNullServiceContext_ThrowsArgumentNullException()
        {
            IServiceContext serviceContext = null;
            var ex = Assert.Throws<ArgumentNullException>(() => serviceContext.Build());
            Assert.Equal("serviceContext", ex.ParamName);
        }

        [Fact]
        public void Build_ReturnsServiceResolver()
        {
            var context = new ServiceContext();
            var resolver = context.Build();
            Assert.NotNull(resolver);
            Assert.IsType<ServiceResolver>(resolver);
        }

        [Fact]
        public void Build_CanResolveRegisteredServices()
        {
            var context = new ServiceContext();
            var instance = new object();
            context.Add(new InstanceServiceDefinition(typeof(object), instance));
            var resolver = context.Build();
            var result = resolver.Resolve(typeof(object));
            Assert.Same(instance, result);
        }
    }
}
