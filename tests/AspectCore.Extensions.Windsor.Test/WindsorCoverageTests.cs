using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Windsor;
using AspectCoreTest.Windsor.Fakes;
using Castle.Core.Internal;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Xunit;

namespace AspectCoreTest.Windsor
{
    public class WindsorCoverageTests
    {
        [Fact]
        public void WindsorServiceResolver_NonIKernelInternal_Throws()
        {
            var fakeKernel = new FakeKernel();
            Assert.Throws<ArgumentException>(() => new WindsorServiceResolver(fakeKernel));
        }

        [Fact]
        public void Interceptor_MethodWithoutAspect_ProceedsWithoutInterception()
        {
            var container = new WindsorContainer().AddAspectCoreFacility();
            container.Register(Component.For<IMixedService>().ImplementedBy<MixedService>().LifestyleTransient());
            var service = container.Resolve<IMixedService>();

            // MethodWithoutAspect has no interceptor attribute, should proceed without interception
            var result = service.MethodWithoutAspect(42);
            Assert.Equal(42, result);
        }

        [Fact]
        public void Interceptor_MethodWithAspect_Intercepts()
        {
            var container = new WindsorContainer().AddAspectCoreFacility();
            container.Register(Component.For<IMixedService>().ImplementedBy<MixedService>().LifestyleTransient());
            var service = container.Resolve<IMixedService>();

            // MethodWithAspect has interceptor attribute, should be intercepted
            var result = service.MethodWithAspect(42);
            Assert.Equal(42, result);
        }

        [Fact]
        public void WindsorAspectBuilderFactory_GetBuilder_TwoArgs_Works()
        {
            var container = new WindsorContainer().AddAspectCoreFacility();
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient());

            var aspectBuilderFactory = container.Resolve<IAspectBuilderFactory>();
            var internalType = typeof(WindsorServiceResolver).Assembly
                .GetType("AspectCore.Extensions.Windsor.WindsorAspectBuilderFactory");
            Assert.NotNull(internalType);

            AspectDelegate complete = ctx => Task.FromResult(0);
            var instance = Activator.CreateInstance(internalType, aspectBuilderFactory, complete);
            Assert.NotNull(instance);

            var serviceMethod = typeof(ICacheService).GetMethod("Get");
            var implementationMethod = typeof(CacheService).GetMethod("Get");
            var getBuilderMethod = internalType.GetMethod("GetBuilder", new[] { typeof(MethodInfo), typeof(MethodInfo) });
            Assert.NotNull(getBuilderMethod);

            var builder = getBuilderMethod.Invoke(instance, new object[] { serviceMethod, implementationMethod });
            Assert.NotNull(builder);
            Assert.IsAssignableFrom<IAspectBuilder>(builder);
        }

        [Fact]
        public void WindsorAspectBuilderFactory_GetBuilder_ThreeArgs_Works()
        {
            var container = new WindsorContainer().AddAspectCoreFacility();
            container.Register(Component.For<ICacheService>().ImplementedBy<CacheService>().LifestyleTransient());

            var aspectBuilderFactory = container.Resolve<IAspectBuilderFactory>();
            var internalType = typeof(WindsorServiceResolver).Assembly
                .GetType("AspectCore.Extensions.Windsor.WindsorAspectBuilderFactory");
            Assert.NotNull(internalType);

            AspectDelegate complete = ctx => Task.FromResult(0);
            var instance = Activator.CreateInstance(internalType, aspectBuilderFactory, complete);
            Assert.NotNull(instance);

            var serviceMethod = typeof(ICacheService).GetMethod("Get");
            var implementationMethod = typeof(CacheService).GetMethod("Get");
            var predicateMethod = typeof(ICacheService).GetMethod("Get");
            var getBuilderMethod = internalType.GetMethod("GetBuilder", new[] { typeof(MethodInfo), typeof(MethodInfo), typeof(MethodInfo) });
            Assert.NotNull(getBuilderMethod);

            var builder = getBuilderMethod.Invoke(instance, new object[] { serviceMethod, implementationMethod, predicateMethod });
            Assert.NotNull(builder);
            Assert.IsAssignableFrom<IAspectBuilder>(builder);
        }
    }

    public interface IMixedService
    {
        [CacheInterceptor]
        int MethodWithAspect(int id);

        int MethodWithoutAspect(int id);
    }

    public class MixedService : IMixedService
    {
        public int MethodWithAspect(int id) => id;

        public int MethodWithoutAspect(int id) => id;
    }

    /// <summary>
    /// Fake IKernel that does NOT implement IKernelInternal.
    /// All members throw NotImplementedException - only used for the cast check in WindsorServiceResolver.
    /// </summary>
    public class FakeKernel : IKernel
    {
        public void AddChildKernel(IKernel kernel) => throw new NotImplementedException();
        public IKernel AddFacility(IFacility facility) => throw new NotImplementedException();
        public IKernel AddFacility() => throw new NotImplementedException();
        public IKernel AddFacility(Action<IKernel> onCreate) => throw new NotImplementedException();
        public IKernel AddFacility<T>() where T : IFacility, new() => throw new NotImplementedException();
        public IKernel AddFacility<T>(Action<T> onCreate) where T : IFacility, new() => throw new NotImplementedException();
        public void AddHandlerSelector(IHandlerSelector selector) => throw new NotImplementedException();
        public void AddHandlersFilter(IHandlersFilter filter) => throw new NotImplementedException();
        public void AddSubSystem(string name, ISubSystem subsystem) { }
        public IComponentModelBuilder ComponentModelBuilder => throw new NotImplementedException();
        public IConfigurationStore ConfigurationStore { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public GraphNode[] GraphNodes => throw new NotImplementedException();
        public IHandlerFactory HandlerFactory => throw new NotImplementedException();
        public IKernel Parent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IProxyFactory ProxyFactory { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IReleasePolicy ReleasePolicy { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IDependencyResolver Resolver => throw new NotImplementedException();
        public IHandler[] GetAssignableHandlers(Type service) => throw new NotImplementedException();
        public IFacility[] GetFacilities() => throw new NotImplementedException();
        public IHandler GetHandler(string name) => throw new NotImplementedException();
        public IHandler GetHandler(Type service) => throw new NotImplementedException();
        public IHandler[] GetHandlers() => throw new NotImplementedException();
        public IHandler[] GetHandlers(Type service) => throw new NotImplementedException();
        public ISubSystem GetSubSystem(string name) => throw new NotImplementedException();
        public bool HasComponent(string name) => false;
        public bool HasComponent(Type service) => false;
        public IKernel Register(params IRegistration[] registrations) => this;
        public void ReleaseComponent(object instance) { }
        public void RemoveChildKernel(IKernel kernel) => throw new NotImplementedException();
        public object Resolve(Type service) => throw new NotImplementedException();
        public object Resolve(Type service, Arguments arguments) => throw new NotImplementedException();
        public object Resolve(string key, Type service) => throw new NotImplementedException();
        public T Resolve<T>(Arguments arguments) => throw new NotImplementedException();
        public T Resolve<T>() => throw new NotImplementedException();
        public T Resolve<T>(string key) => throw new NotImplementedException();
        public T Resolve<T>(string key, Arguments arguments) => throw new NotImplementedException();
        public object Resolve(string key, Type service, Arguments arguments) => throw new NotImplementedException();
        public Array ResolveAll(Type service) => throw new NotImplementedException();
        public Array ResolveAll(Type service, Arguments arguments) => throw new NotImplementedException();
        public TService[] ResolveAll<TService>() => throw new NotImplementedException();
        public TService[] ResolveAll<TService>(Arguments arguments) => throw new NotImplementedException();

        // IKernelEvents
        public event ComponentDataDelegate ComponentRegistered;
        public event ComponentModelDelegate ComponentModelCreated;
        public event EventHandler AddedAsChildKernel;
        public event EventHandler RemovedAsChildKernel;
        public event ComponentInstanceDelegate ComponentCreated;
        public event ComponentInstanceDelegate ComponentDestroyed;
        public event HandlerDelegate HandlerRegistered;
        public event HandlersChangedDelegate HandlersChanged;
        public event DependencyDelegate DependencyResolving;
        public event EventHandler RegistrationCompleted;
        public event ServiceDelegate EmptyCollectionResolving;

        // IDisposable
        public void Dispose() { }
    }
}
