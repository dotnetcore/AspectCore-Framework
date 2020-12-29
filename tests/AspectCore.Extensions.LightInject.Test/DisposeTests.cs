using System;
using System.Collections.Generic;
using System.Linq;
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

        private static IServiceResolver Create(Action<ServiceContainer> action, bool registerServiceBeforeAop)
        {
            var container = new ServiceContainer();
            if (registerServiceBeforeAop)
            {
                action(container);
                container.RegisterDynamicProxy();
            }
            else
            {
                container.RegisterDynamicProxy();
                action(container);
            }
            return container.GetInstance<IServiceResolver>();
        }

        private static IServiceResolver Create(ILifetime lifetime, bool registerServiceBeforeAop, bool byFac)
        {
            return byFac
                ? Create(c => c.Register<DisposableService>(lifetime), registerServiceBeforeAop)
                : Create(c => c.Register<DisposableService>(s => new DisposableService(), lifetime), registerServiceBeforeAop);
        }

        private static bool[] Bools { get; } = { true, false };

        public static IEnumerable<object[]> TestCases { get; } = Bools
            .SelectMany(m => Bools, (x, y) => (x, y))
            .Select(m => new object[] { m.x, m.y });

        [Theory]
        [MemberData(nameof(TestCases))]
        public void Transient_Test(bool registerServiceBeforeAop, bool byFac)
        {
            var resolver = Create((ILifetime)null, registerServiceBeforeAop, byFac);
            var service = resolver.Resolve<DisposableService>();
            resolver.Dispose();
            Assert.Equal(0, service.DisposeTimes);
        }

        [Theory]
        [MemberData(nameof(TestCases))]
        public void RequestLifeTime_Test(bool registerServiceBeforeAop, bool byFac)
        {
            var resolver = Create(new PerRequestLifeTime(), registerServiceBeforeAop, byFac);
            var scope = resolver.CreateScope();
            var service = scope.Resolve<DisposableService>();
            scope.Dispose();
            Assert.Equal(1, service.DisposeTimes);

            resolver.Dispose();
            Assert.Equal(1, service.DisposeTimes);
        }

        [Theory]
        [MemberData(nameof(TestCases))]
        public void Singleton_Test(bool registerServiceBeforeAop, bool byFac)
        {
            var resolver = Create(new PerContainerLifetime(), registerServiceBeforeAop, byFac);
            var service = resolver.Resolve<DisposableService>();
            resolver.Dispose();
            Assert.Equal(1, service.DisposeTimes);
        }

        [Theory]
        [MemberData(nameof(TestCases))]
        public void Scope_Test(bool registerServiceBeforeAop, bool byFac)
        {
            var resolver = Create(new PerScopeLifetime(), registerServiceBeforeAop, byFac);
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
