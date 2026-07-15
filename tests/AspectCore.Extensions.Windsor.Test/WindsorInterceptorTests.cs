using System;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Windsor;
using AspectCoreTest.Windsor.Fakes;
using Castle.MicroKernel.Lifestyle;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Xunit;

namespace AspectCoreTest.Windsor
{
    public class WindsorInterceptorTests
    {
        private static IWindsorContainer CreateContainer()
        {
            return new WindsorContainer().AddAspectCoreFacility();
        }

        [Fact]
        public void Interceptor_SyncMethod_InterceptsCall()
        {
            var container = CreateContainer();
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient());
            var service = container.Resolve<ICacheService>();

            var result1 = service.Get(1);
            var result2 = service.Get(1);
            Assert.Equal(result1.Id, result2.Id);
            Assert.Equal(result1.Version, result2.Version);
        }

        [Fact]
        public void Interceptor_SyncMethod_DifferentIds_ReturnDifferentResults()
        {
            var container = CreateContainer();
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient());
            var service = container.Resolve<ICacheService>();

            var result1 = service.Get(1);
            var result2 = service.Get(2);
            Assert.NotEqual(result1.Id, result2.Id);
        }

        [Fact]
        public async Task Interceptor_AsyncMethod_InterceptsCall()
        {
            var container = CreateContainer();
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient());
            var service = container.Resolve<ICacheService>();

            var result1 = await service.GetAsync(1);
            var result2 = await service.GetAsync(1);
            Assert.Equal(result1.Id, result2.Id);
            Assert.Equal(result1.Version, result2.Version);
        }

        [Fact]
        public async Task Interceptor_AsyncMethod_DifferentIds_ReturnDifferentResults()
        {
            var container = CreateContainer();
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient());
            var service = container.Resolve<ICacheService>();

            var result1 = await service.GetAsync(1);
            var result2 = await service.GetAsync(2);
            Assert.NotEqual(result1.Id, result2.Id);
        }

        [Fact]
        public void Interceptor_MultipleServices_AllIntercepted()
        {
            var container = CreateContainer();
            container.Register(
                Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient(),
                Component.For<IController>().ImplementedBy<Controller>().LifestyleTransient());

            var cache = container.Resolve<ICacheService>();
            var controller = container.Resolve<IController>();

            Assert.NotNull(cache.Get(1));
            Assert.NotNull(controller.Execute());
        }

        [Fact]
        public void Interceptor_TransientService_DifferentInstances()
        {
            var container = CreateContainer();
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient());

            var s1 = container.Resolve<ICacheService>();
            var s2 = container.Resolve<ICacheService>();
            Assert.NotSame(s1, s2);
        }

        [Fact]
        public void Interceptor_SingletonService_SameInstance()
        {
            var container = CreateContainer();
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleSingleton());

            var s1 = container.Resolve<ICacheService>();
            var s2 = container.Resolve<ICacheService>();
            Assert.Same(s1, s2);
        }

        [Fact]
        public void Interceptor_ScopedService_DifferentPerScope()
        {
            var container = CreateContainer();
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleScoped());

            ICacheService s1;
            ICacheService s2;
            using (container.BeginScope())
            {
                s1 = container.Resolve<ICacheService>();
            }
            using (container.BeginScope())
            {
                s2 = container.Resolve<ICacheService>();
            }
            Assert.NotSame(s1, s2);
        }

        [Fact]
        public void Interceptor_WithDelegateInterceptor_Intercepts()
        {
            var container = new WindsorContainer().AddAspectCoreFacility(config =>
            {
                config.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    await next(ctx);
                    ctx.ReturnValue = new Model { Id = 999, Version = Guid.NewGuid() };
                }, Predicates.ForNameSpace("AspectCoreTest.Windsor.Fakes"));
            });
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient());
            var service = container.Resolve<ICacheService>();

            var result = service.Get(1);
            Assert.Equal(999, result.Id);
        }

        [Fact]
        public void Interceptor_WithMultipleInterceptors_AllRun()
        {
            var container = new WindsorContainer().AddAspectCoreFacility(config =>
            {
                config.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    await next(ctx);
                    if (ctx.ReturnValue is Model m)
                    {
                        m.Id += 100;
                    }
                }, Predicates.ForNameSpace("AspectCoreTest.Windsor.Fakes"));
            });
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient());
            var service = container.Resolve<ICacheService>();

            var result = service.Get(1);
            Assert.Equal(101, result.Id);
        }

        [Fact]
        public void CompatibleCollectionResolver_ResolvesEnumerable()
        {
            var container = CreateContainer();
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient());
            var services = container.ResolveAll<ICacheService>();
            Assert.NotNull(services);
        }

        [Fact]
        public void CompatibleCollectionResolver_ResolvesMultipleServices()
        {
            var container = CreateContainer();
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient());
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService2>().LifestyleTransient());
            var services = container.ResolveAll<ICacheService>();
            Assert.Equal(2, services.Length);
        }

        [Fact]
        public void NonAspectService_NotProxied()
        {
            var container = CreateContainer();
            container.Register(Component.For<INonAspectService>().ImplementedBy<NonAspectService>().LifestyleTransient());
            var service = container.Resolve<INonAspectService>();
            Assert.NotNull(service);
            Assert.Equal("non-aspect", service.GetValue());
        }

        [Fact]
        public void Container_Dispose_CleansUp()
        {
            var container = CreateContainer();
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient());
            var service = container.Resolve<ICacheService>();
            Assert.NotNull(service);
            container.Dispose();
        }

        public class CacheService2 : ICacheService
        {
            public Model Get(int id) => new Model { Id = id, Version = Guid.NewGuid() };
            public Task<Model> GetAsync(int id) => Task.FromResult(new Model { Id = id, Version = Guid.NewGuid() });
        }

        public interface INonAspectService
        {
            string GetValue();
        }

        public class NonAspectService : INonAspectService
        {
            public string GetValue() => "non-aspect";
        }
    }
}
