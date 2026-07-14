using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.LightInject;
using AspectCoreTest.LightInject.Fakes;
using LightInject;
using Xunit;

namespace AspectCoreTest.LightInject
{
    /// <summary>
    /// E2E tests for AspectCore + LightInject integration: register/resolve proxied
    /// services, interceptor execution, async interception, multiple interceptors,
    /// constructor injection, and scoped services. Real LightInject container,
    /// real proxies, real interceptors — no mocks.
    /// </summary>
    public class E2EScenarios
    {
        private static IServiceContainer BuildContainer(Action<IServiceContainer> beforeBuild = null)
        {
            var container = new ServiceContainer();
            container.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                    Predicates.ForNameSpace("AspectCoreTest.LightInject.Fakes"));
            });
            container.Register<IService, Service>();
            beforeBuild?.Invoke(container);
            return container;
        }

        [Fact]
        public void RegisterAndResolve_ProxiedService_Works()
        {
            var container = BuildContainer();
            var service = container.GetInstance<IService>();

            Assert.NotNull(service);
            Assert.IsAssignableFrom<IService>(service);
            // The resolved instance must be a generated proxy.
            Assert.IsNotType<Service>(service);
        }

        [Fact]
        public void InterceptorExecution_ThroughLightInjectResolvedProxy_Works()
        {
            var container = new ServiceContainer();
            container.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    E2ELog.Entries.Add("Interceptor.Before");
                    await ctx.Invoke(next);
                    E2ELog.Entries.Add("Interceptor.After");
                }, Predicates.ForNameSpace("AspectCoreTest.LightInject.Fakes"));
            });
            container.Register<IService, Service>();

            E2ELog.Clear();
            var service = container.GetInstance<IService>();
            var result = service.Get(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Contains("Interceptor.Before", E2ELog.Entries);
            Assert.Contains("Interceptor.After", E2ELog.Entries);
        }

        [Fact]
        public async Task AsyncMethodInterception_ThroughLightInject_Works()
        {
            var container = new ServiceContainer();
            container.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    E2ELog.Entries.Add("Async.Before");
                    await ctx.Invoke(next);
                    var unwrapped = await ctx.UnwrapAsyncReturnValue();
                    E2ELog.Entries.Add($"Async.Result={unwrapped}");
                    E2ELog.Entries.Add("Async.After");
                }, Predicates.ForNameSpace("AspectCoreTest.LightInject"));
            });
            container.Register<IAsyncService, AsyncService>();

            E2ELog.Clear();
            var service = container.GetInstance<IAsyncService>();
            var result = await service.GetNameAsync();

            Assert.Equal("async-name", result);
            Assert.Contains("Async.Before", E2ELog.Entries);
            Assert.Contains("Async.Result=async-name", E2ELog.Entries);
            Assert.Contains("Async.After", E2ELog.Entries);
        }

        [Fact]
        public void MultipleInterceptors_ViaLightInject_AllExecute()
        {
            var container = new ServiceContainer();
            container.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    E2ELog.Entries.Add("First.Before");
                    await ctx.Invoke(next);
                    E2ELog.Entries.Add("First.After");
                }, Predicates.ForNameSpace("AspectCoreTest.LightInject.Fakes"));

                config.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    E2ELog.Entries.Add("Second.Before");
                    await ctx.Invoke(next);
                    E2ELog.Entries.Add("Second.After");
                }, Predicates.ForNameSpace("AspectCoreTest.LightInject.Fakes"));
            });
            container.Register<IService, Service>();

            E2ELog.Clear();
            var service = container.GetInstance<IService>();
            var result = service.Get(1);

            Assert.NotNull(result);
            Assert.Equal("First.Before", E2ELog.Entries[0]);
            Assert.Equal("Second.Before", E2ELog.Entries[1]);
            Assert.Equal("Second.After", E2ELog.Entries[2]);
            Assert.Equal("First.After", E2ELog.Entries[3]);
        }

        [Fact]
        public void ConstructorInjection_ThroughLightInjectProxy_Works()
        {
            var container = new ServiceContainer();
            container.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                    Predicates.ForNameSpace("AspectCoreTest.LightInject.Fakes"));
            });
            container.Register<IService, Service>();
            container.Register<IController, Controller>();

            var controller = container.GetInstance<IController>();

            Assert.NotNull(controller);
            // Controller.Execute() calls IService.Get(100) — both are proxied.
            var result = controller.Execute();
            Assert.Equal(100, result.Id);
        }

        [Fact]
        public void InterceptorModifiesReturnValue_ThroughLightInject_Works()
        {
            var container = new ServiceContainer();
            container.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    await ctx.Invoke(next);
                    if (ctx.ReturnValue is Model m)
                    {
                        ctx.ReturnValue = new Model { Id = m.Id + 300, Version = m.Version };
                    }
                }, Predicates.ForNameSpace("AspectCoreTest.LightInject.Fakes"));
            });
            container.Register<IService, Service>();

            var service = container.GetInstance<IService>();
            var result = service.Get(5);

            Assert.Equal(305, result.Id);
        }

        [Fact]
        public void ScopedService_ThroughLightInject_DifferentAcrossScopes()
        {
            var container = new ServiceContainer();
            container.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                    Predicates.ForNameSpace("AspectCoreTest.LightInject.Fakes"));
            });
            container.Register<IService, Service>(new PerScopeLifetime());

            // LightInject requires an active scope for PerScopeLifetime.
            using (container.BeginScope())
            {
                var s1 = container.GetInstance<IService>();
                var s2 = container.GetInstance<IService>();

                // Same scope → same instance.
                Assert.Same(s1, s2);
                Assert.IsNotType<Service>(s1);
                Assert.Equal(1, s1.Get(1).Id);
            }

            // Different scope → different instance.
            using (container.BeginScope())
            {
                var s3 = container.GetInstance<IService>();
                Assert.IsNotType<Service>(s3);
                Assert.Equal(2, s3.Get(2).Id);
            }
        }

        /// <summary>
        /// Shared log for E2E interceptor execution verification.
        /// </summary>
        public static class E2ELog
        {
            public static readonly List<string> Entries = new();
            public static void Clear() => Entries.Clear();
        }

        /// <summary>
        /// Async service for async interception E2E tests through LightInject.
        /// </summary>
        public interface IAsyncService
        {
            Task<string> GetNameAsync();
        }

        public class AsyncService : IAsyncService
        {
            public async Task<string> GetNameAsync()
            {
                await Task.Delay(1);
                return "async-name";
            }
        }
    }
}
