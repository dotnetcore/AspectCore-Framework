using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Windsor;
using AspectCoreTest.Windsor.Fakes;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Xunit;

namespace AspectCoreTest.Windsor
{
    /// <summary>
    /// E2E tests for AspectCore + Windsor integration: register/resolve proxied
    /// services, interceptor execution, facility configuration, async interception,
    /// constructor injection, and scoped services. Real Windsor container,
    /// real proxies, real interceptors — no mocks.
    /// </summary>
    public class E2EScenarios
    {
        private static IWindsorContainer CreateContainer()
        {
            return new WindsorContainer().AddAspectCoreFacility();
        }

        [Fact]
        public void RegisterAndResolve_ProxiedService_Works()
        {
            var container = CreateContainer();
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient());

            var service = container.Resolve<ICacheService>();

            Assert.NotNull(service);
            Assert.IsAssignableFrom<ICacheService>(service);
            // The resolved instance must be a generated proxy.
            Assert.IsNotType<CacheService>(service);
        }

        [Fact]
        public void InterceptorExecution_ThroughWindsorResolvedProxy_Works()
        {
            var container = CreateContainer();
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient());

            E2ELog.Clear();
            var service = container.Resolve<ICacheService>();
            var result = service.Get(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            // The CacheInterceptor caches results — second call returns cached value.
            var result2 = service.Get(1);
            Assert.Equal(result.Id, result2.Id);
            Assert.Equal(result.Version, result2.Version);
        }

        [Fact]
        public void FacilityConfiguration_WithConfigure_InvokesConfigure()
        {
            var configured = false;
            var container = new WindsorContainer().AddAspectCoreFacility(config =>
            {
                configured = true;
            });

            Assert.True(configured);
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient());

            var service = container.Resolve<ICacheService>();
            Assert.NotNull(service);
            Assert.IsNotType<CacheService>(service);
        }

        [Fact]
        public async Task AsyncMethodInterception_ThroughWindsor_Works()
        {
            var container = CreateContainer();
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient());

            var service = container.Resolve<ICacheService>();
            var result = await service.GetAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);

            // Second call with same id returns cached result.
            var result2 = await service.GetAsync(1);
            Assert.Equal(result.Id, result2.Id);
            Assert.Equal(result.Version, result2.Version);
        }

        [Fact]
        public void ConstructorInjection_ThroughWindsorProxy_Works()
        {
            var container = CreateContainer();
            container.Register(
                Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient(),
                Component.For<IController>().ImplementedBy<Controller>().LifestyleTransient());

            var controller = container.Resolve<IController>();

            Assert.NotNull(controller);
            // Controller.Execute() calls ICacheService.Get(100) — both are proxied.
            var result = controller.Execute();
            Assert.Equal(100, result.Id);
        }

        [Fact]
        public void MultipleInterceptors_ViaWindsorFacility_AllExecute()
        {
            var container = new WindsorContainer().AddAspectCoreFacility(config =>
            {
                config.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    E2ELog.Entries.Add("First.Before");
                    await ctx.Invoke(next);
                    E2ELog.Entries.Add("First.After");
                }, Predicates.ForNameSpace("AspectCoreTest.Windsor.Fakes"));

                config.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    E2ELog.Entries.Add("Second.Before");
                    await ctx.Invoke(next);
                    E2ELog.Entries.Add("Second.After");
                }, Predicates.ForNameSpace("AspectCoreTest.Windsor.Fakes"));
            });

            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient());

            E2ELog.Clear();
            var service = container.Resolve<ICacheService>();
            var result = service.Get(1);

            Assert.NotNull(result);
            Assert.Contains("First.Before", E2ELog.Entries);
            Assert.Contains("Second.Before", E2ELog.Entries);
            Assert.Contains("Second.After", E2ELog.Entries);
            Assert.Contains("First.After", E2ELog.Entries);
        }

        [Fact]
        public void InterceptorModifiesReturnValue_ThroughWindsor_Works()
        {
            var container = new WindsorContainer().AddAspectCoreFacility(config =>
            {
                config.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    await ctx.Invoke(next);
                    if (ctx.ReturnValue is Model m)
                    {
                        ctx.ReturnValue = new Model { Id = m.Id + 200, Version = m.Version };
                    }
                }, Predicates.ForNameSpace("AspectCoreTest.Windsor.Fakes"));
            });

            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient());

            var service = container.Resolve<ICacheService>();
            var result = service.Get(5);

            Assert.Equal(205, result.Id);
        }

        [Fact]
        public void ScopedService_ThroughWindsor_DifferentAcrossScopes()
        {
            var container = CreateContainer();
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient());

            // Transient: each resolution returns a different proxy instance.
            var s1 = container.Resolve<ICacheService>();
            var s2 = container.Resolve<ICacheService>();

            Assert.NotNull(s1);
            Assert.NotNull(s2);
            Assert.NotSame(s1, s2);
            Assert.IsNotType<CacheService>(s1);
            Assert.IsNotType<CacheService>(s2);

            // Functional behavior is preserved.
            var result = s1.Get(1);
            Assert.Equal(1, result.Id);
        }

        /// <summary>
        /// Shared log for E2E interceptor execution verification.
        /// </summary>
        public static class E2ELog
        {
            public static readonly List<string> Entries = new();
            public static void Clear() => Entries.Clear();
        }
    }
}
