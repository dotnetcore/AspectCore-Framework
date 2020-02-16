using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.DependencyInjection;
using AspectCore.Extensions.LightInject;
using LightInject;
using Xunit;

namespace AspectCoreTest.LightInject
{
    public class DisposeTests
    {
        public class DisposableService : IDisposable
        {
            public int DisposeTimes { get; private set; }

            public void Dispose()
            {
                DisposeTimes++;
            }
        }

        private static IServiceResolver Create(ILifetime lifetime, bool registerServiceBeforeAop)
        {
            var container = new ServiceContainer();
            if (registerServiceBeforeAop)
            {
                container.Register<DisposableService>(lifetime);
                container.RegisterDynamicProxy();
            }
            else
            {
                container.RegisterDynamicProxy();
                container.Register<DisposableService>(lifetime);
            }
            return container.GetInstance<IServiceResolver>();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Transient_Test(bool registerServiceBeforeAop)
        {
            var resolver = Create(null, registerServiceBeforeAop);
            var service = resolver.Resolve<DisposableService>();
            resolver.Dispose();
            Assert.Equal(0, service.DisposeTimes);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RequestLifeTime_Test(bool registerServiceBeforeAop)
        {
            var resolver = Create(new PerRequestLifeTime(), registerServiceBeforeAop);
            var scope = resolver.CreateScope();
            var service = scope.Resolve<DisposableService>();
            scope.Dispose();
            Assert.Equal(1, service.DisposeTimes);

            resolver.Dispose();
            Assert.Equal(1, service.DisposeTimes);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Singleton_Test(bool registerServiceBeforeAop)
        {
            var resolver = Create(new PerContainerLifetime(), registerServiceBeforeAop);
            var service = resolver.Resolve<DisposableService>();
            resolver.Dispose();
            Assert.Equal(1, service.DisposeTimes);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Scope_Test(bool registerServiceBeforeAop)
        {
            var resolver = Create(new PerScopeLifetime(), registerServiceBeforeAop);
            var outerScope = resolver.CreateScope();
            var serviceFromOuter = outerScope.Resolve<DisposableService>();
            var innerScope = resolver.CreateScope();
            var serviceFromInner = innerScope.Resolve<DisposableService>();

            innerScope.Dispose();
            Assert.Equal(1, serviceFromInner.DisposeTimes);

            outerScope.Dispose();
            Assert.Equal(1, serviceFromOuter.DisposeTimes);

            resolver.Dispose();

            Assert.Equal(1, serviceFromInner.DisposeTimes);
            Assert.Equal(1, serviceFromOuter.DisposeTimes);
        }
    }
}
