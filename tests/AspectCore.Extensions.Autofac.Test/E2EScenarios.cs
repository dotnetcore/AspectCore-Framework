using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Autofac;
using AspectCore.Extensions.Test.Fakes;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCoreTest.Autofac
{
    /// <summary>
    /// E2E tests for AspectCore + Autofac integration: register/resolve proxied
    /// services, interceptor execution, scoped services, multiple interceptors,
    /// async interception, and keyed service resolution. Real Autofac container,
    /// real proxies, real interceptors — no mocks.
    /// </summary>
    public class E2EScenarios
    {
        private static IContainer BuildContainer(Action<ContainerBuilder> beforeBuild = null)
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                    Predicates.ForNameSpace("AspectCore.Extensions.Test.Fakes"));
            });
            builder.RegisterType<Service>().As<IService>();
            beforeBuild?.Invoke(builder);
            return builder.Build();
        }

        [Fact]
        public void RegisterAndResolve_ProxiedService_Works()
        {
            var container = BuildContainer();
            var service = container.Resolve<IService>();

            Assert.NotNull(service);
            Assert.IsAssignableFrom<IService>(service);
            // The resolved instance must be a generated proxy.
            Assert.True(service.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
        }

        [Fact]
        public void InterceptorExecution_ThroughAutofacResolvedProxy_Works()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    E2ELog.Entries.Add("Interceptor.Before");
                    await ctx.Invoke(next);
                    E2ELog.Entries.Add("Interceptor.After");
                }, Predicates.ForNameSpace("AspectCore.Extensions.Test.Fakes"));
            });
            builder.RegisterType<Service>().As<IService>();
            var container = builder.Build();

            E2ELog.Clear();
            var service = container.Resolve<IService>();
            var result = service.Get(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Contains("Interceptor.Before", E2ELog.Entries);
            Assert.Contains("Interceptor.After", E2ELog.Entries);
        }

        [Fact]
        public void ScopedService_ThroughAutofac_DifferentAcrossScopes()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                    Predicates.ForNameSpace("AspectCore.Extensions.Test.Fakes"));
            });
            builder.RegisterType<Service>().As<IService>().InstancePerLifetimeScope();
            var container = builder.Build();

            using var scope1 = container.BeginLifetimeScope();
            var s1 = scope1.Resolve<IService>();
            var s2 = scope1.Resolve<IService>();

            // Same scope → same proxy instance.
            Assert.Same(s1, s2);

            using var scope2 = container.BeginLifetimeScope();
            var s3 = scope2.Resolve<IService>();

            // Different scope → different proxy instance.
            Assert.NotSame(s1, s3);

            // All are proxies.
            Assert.True(s1.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
            Assert.True(s3.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
        }

        [Fact]
        public void MultipleInterceptors_ViaAutofacConfiguration_AllExecute()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    E2ELog.Entries.Add("First.Before");
                    await ctx.Invoke(next);
                    E2ELog.Entries.Add("First.After");
                }, Predicates.ForNameSpace("AspectCore.Extensions.Test.Fakes"));

                config.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    E2ELog.Entries.Add("Second.Before");
                    await ctx.Invoke(next);
                    E2ELog.Entries.Add("Second.After");
                }, Predicates.ForNameSpace("AspectCore.Extensions.Test.Fakes"));
            });
            builder.RegisterType<Service>().As<IService>();
            var container = builder.Build();

            E2ELog.Clear();
            var service = container.Resolve<IService>();
            var result = service.Get(1);

            Assert.NotNull(result);
            Assert.Equal("First.Before", E2ELog.Entries[0]);
            Assert.Equal("Second.Before", E2ELog.Entries[1]);
            Assert.Equal("Second.After", E2ELog.Entries[2]);
            Assert.Equal("First.After", E2ELog.Entries[3]);
        }

        [Fact]
        public async Task AsyncMethodInterception_ThroughAutofac_Works()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    E2ELog.Entries.Add("Async.Before");
                    await ctx.Invoke(next);
                    var unwrapped = await ctx.UnwrapAsyncReturnValue();
                    E2ELog.Entries.Add($"Async.Result={unwrapped}");
                    E2ELog.Entries.Add("Async.After");
                }, Predicates.ForNameSpace("AspectCoreTest.Autofac"));
            });
            builder.RegisterType<AsyncService>().As<IAsyncService>();
            var container = builder.Build();

            E2ELog.Clear();
            var service = container.Resolve<IAsyncService>();
            var result = await service.GetNameAsync();

            Assert.Equal("async-name", result);
            Assert.Contains("Async.Before", E2ELog.Entries);
            Assert.Contains("Async.Result=async-name", E2ELog.Entries);
            Assert.Contains("Async.After", E2ELog.Entries);
        }

        [Fact]
        public void InterceptorModifiesReturnValue_ThroughAutofac_Works()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    await ctx.Invoke(next);
                    // Modify the return value after the method completes.
                    if (ctx.ReturnValue is Model m)
                    {
                        ctx.ReturnValue = new Model { Id = m.Id + 100, Version = m.Version };
                    }
                }, Predicates.ForNameSpace("AspectCore.Extensions.Test.Fakes"));
            });
            builder.RegisterType<Service>().As<IService>();
            var container = builder.Build();

            var service = container.Resolve<IService>();
            var result = service.Get(5);

            Assert.Equal(105, result.Id);
        }

        [Fact]
        public void NamedServiceResolution_ThroughAutofac_Works()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                    Predicates.ForNameSpace("AspectCore.Extensions.Test.Fakes"));
            });
            builder.RegisterType<Service>().Named<IService>("named");
            var container = builder.Build();

            var service = container.ResolveNamed<IService>("named");

            Assert.NotNull(service);
            Assert.IsAssignableFrom<IService>(service);
            Assert.True(service.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
            var result = service.Get(1);
            Assert.Equal(1, result.Id);
        }

#if NET8_0_OR_GREATER
        [Fact]
        public void KeyedServiceResolution_ThroughAutofac_Works()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                    Predicates.ForNameSpace("AspectCore.Extensions.Test.Fakes"));
            });
            var key = "my-key";
            builder.RegisterType<Service>().Keyed<IService>(key);
            var container = builder.Build();

            var service = container.ResolveKeyed<IService>(key);

            Assert.NotNull(service);
            Assert.IsAssignableFrom<IService>(service);
            Assert.True(service.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
            var result = service.Get(42);
            Assert.Equal(42, result.Id);
        }
#endif

        [Fact]
        public void ConstructorInjection_ThroughAutofacProxy_Works()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                    Predicates.ForNameSpace("AspectCore.Extensions.Test.Fakes"));
            });
            builder.RegisterType<Service>().As<IService>();
            builder.RegisterType<Controller>().As<IController>();
            var container = builder.Build();

            var controller = container.Resolve<IController>();

            Assert.NotNull(controller);
            // Controller.Execute() calls IService.Get(100) — both are proxied.
            var result = controller.Execute();
            Assert.Equal(100, result.Id);
        }

        [Fact]
        public void SingletonService_ThroughAutofac_SameInstanceEverywhere()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                    Predicates.ForNameSpace("AspectCore.Extensions.Test.Fakes"));
            });
            builder.RegisterType<Service>().As<IService>().SingleInstance();
            var container = builder.Build();

            var s1 = container.Resolve<IService>();
            var s2 = container.Resolve<IService>();

            // Singleton: same proxy instance.
            Assert.Same(s1, s2);
            Assert.True(s1.GetType().GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));

            // Even from a child scope, the singleton returns the same instance.
            using var scope = container.BeginLifetimeScope();
            var s3 = scope.Resolve<IService>();
            Assert.Same(s1, s3);
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
        /// Async service for async interception E2E tests through Autofac.
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
