using System;
using System.Collections;
using System.Collections.Generic;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DependencyInjection
{
    public class ConfigurationExtensionsTests
    {
        #region Configure

        [Fact]
        public void Configure_WithNullServiceContext_ThrowsArgumentNullException()
        {
            IServiceContext serviceContext = null;
            var ex = Assert.Throws<ArgumentNullException>(() => serviceContext.Configure(config => { }));
            Assert.Equal("serviceContext", ex.ParamName);
        }

        [Fact]
        public void Configure_WithNullConfigure_DoesNotThrow()
        {
            var context = new FakeServiceContext();
            var result = context.Configure(null);
            Assert.Same(context, result);
        }

        [Fact]
        public void Configure_WithConfigureAction_InvokesActionWithConfiguration()
        {
            var context = new FakeServiceContext();
            IAspectConfiguration receivedConfig = null;

            var result = context.Configure(c =>
            {
                receivedConfig = c;
            });

            Assert.Same(context.Configuration, receivedConfig);
        }

        [Fact]
        public void Configure_ReturnsSameServiceContext()
        {
            var context = new FakeServiceContext();
            var result = context.Configure(c => { });
            Assert.Same(context, result);
        }

        [Fact]
        public void Configure_ActionCanModifyConfiguration()
        {
            var context = new FakeServiceContext();
            context.Configure(c =>
            {
                c.ThrowAspectException = true;
            });
            Assert.True(context.Configuration.ThrowAspectException);
        }

        [Fact]
        public void Configure_MultipleActions_AllAreInvoked()
        {
            var context = new FakeServiceContext();
            int callCount = 0;

            context.Configure(c => callCount++);
            context.Configure(c => callCount++);

            Assert.Equal(2, callCount);
        }

        #endregion

        #region Test Types

        private class FakeServiceContext : IServiceContext
        {
            public IAspectConfiguration Configuration { get; } = new AspectConfiguration();

            public ILifetimeServiceContext Singletons => null;

            public ILifetimeServiceContext Scopeds => null;

            public ILifetimeServiceContext Transients => null;

            public int Count => 0;

            public void Add(ServiceDefinition item) { }

            public bool Remove(ServiceDefinition item) => false;

            public bool Contains(Type serviceType) => false;

            public IEnumerator<ServiceDefinition> GetEnumerator()
            {
                yield break;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        #endregion
    }
}
