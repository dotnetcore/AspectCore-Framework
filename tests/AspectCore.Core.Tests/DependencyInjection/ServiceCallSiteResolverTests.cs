using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DependencyInjection
{
    public class ServiceCallSiteResolverTests
    {
        private static ServiceResolver CreateServiceResolver(IEnumerable<ServiceDefinition> services = null)
        {
            var context = new ServiceContext(services ?? new List<ServiceDefinition>());
            return new ServiceResolver(context);
        }

        [Fact]
        public void Resolve_WithInstanceServiceDefinition_ReturnsInstance()
        {
            var instance = new object();
            var def = new InstanceServiceDefinition(typeof(object), instance);
            var resolver = CreateServiceResolver();
            var callSite = CreateCallSiteResolver();
            var result = callSite.Resolve(def)(resolver);
            Assert.Same(instance, result);
        }

        [Fact]
        public void Resolve_WithDelegateServiceDefinition_InvokesDelegate()
        {
            var expected = new object();
            Func<IServiceResolver, object> impl = r => expected;
            var def = new DelegateServiceDefinition(typeof(object), impl, Lifetime.Transient);
            var resolver = CreateServiceResolver();
            var callSite = CreateCallSiteResolver();
            var result = callSite.Resolve(def)(resolver);
            Assert.Same(expected, result);
        }

        [Fact]
        public void Resolve_WithTypeServiceDefinition_CreatesInstance()
        {
            var def = new TypeServiceDefinition(typeof(IDisposable), typeof(DisposableImpl), Lifetime.Transient);
            var resolver = CreateServiceResolver();
            var callSite = CreateCallSiteResolver();
            var result = callSite.Resolve(def)(resolver);
            Assert.NotNull(result);
            Assert.IsType<DisposableImpl>(result);
        }

        [Fact]
        public void Resolve_WithEnumerableServiceDefinition_ReturnsArray()
        {
            var elementDefs = new ServiceDefinition[]
            {
                new TypeServiceDefinition(typeof(IDisposable), typeof(DisposableImpl), Lifetime.Transient),
                new InstanceServiceDefinition(typeof(IDisposable), new DisposableImpl()),
            };
            var def = new EnumerableServiceDefinition(
                typeof(IEnumerable<IDisposable>), typeof(IDisposable), elementDefs);
            var resolver = CreateServiceResolver();
            var callSite = CreateCallSiteResolver();
            var result = callSite.Resolve(def)(resolver);
            Assert.NotNull(result);
            var array = Assert.IsType<IDisposable[]>(result);
            Assert.Equal(2, array.Length);
        }

        [Fact]
        public void Resolve_WithManyEnumerableServiceDefinition_ReturnsManyEnumerable()
        {
            var elementDefs = new ServiceDefinition[]
            {
                new TypeServiceDefinition(typeof(IDisposable), typeof(DisposableImpl), Lifetime.Transient),
            };
            var def = new ManyEnumerableServiceDefinition(
                typeof(IManyEnumerable<IDisposable>), typeof(IDisposable), elementDefs);
            var resolver = CreateServiceResolver();
            var callSite = CreateCallSiteResolver();
            var result = callSite.Resolve(def)(resolver);
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IManyEnumerable<IDisposable>>(result);
        }

        [Fact]
        public void Resolve_WithProxyServiceDefinition_ForInterface_CreatesProxy()
        {
            // Configure an interceptor so the service gets validated as needing a proxy
            var config = new AspectConfiguration();
            config.Interceptors.Add(new TestInterceptorFactory());

            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(ITestService), typeof(TestServiceImpl), Lifetime.Transient)
            };
            var context = new ServiceContext(services, config);
            var table = new ServiceTable(context);
            table.Populate(context);
            var proxyDef = table.TryGetService(typeof(ITestService));
            Assert.NotNull(proxyDef);
            Assert.IsType<ProxyServiceDefinition>(proxyDef);

            var resolver = new ServiceResolver(context);
            var callSite = new ServiceCallSiteResolver(table);
            var result = callSite.Resolve(proxyDef)(resolver);
            Assert.NotNull(result);
            Assert.IsAssignableFrom<ITestService>(result);
        }

        [Fact]
        public void Resolve_WithUnknownServiceDefinition_ReturnsNull()
        {
            var def = new UnknownServiceDefinition();
            var resolver = CreateServiceResolver();
            var callSite = CreateCallSiteResolver();
            var result = callSite.Resolve(def)(resolver);
            Assert.Null(result);
        }

        [Fact]
        public void Resolve_CachesResultForSameService()
        {
            var def = new TypeServiceDefinition(typeof(IDisposable), typeof(DisposableImpl), Lifetime.Transient);
            var callSite = CreateCallSiteResolver();
            var result1 = callSite.Resolve(def);
            var result2 = callSite.Resolve(def);
            Assert.Same(result1, result2);
        }

        private static ServiceCallSiteResolver CreateCallSiteResolver()
        {
            var context = new ServiceContext();
            var table = new ServiceTable(context);
            return new ServiceCallSiteResolver(table);
        }

        private class DisposableImpl : IDisposable
        {
            public void Dispose() { }
        }

        public interface ITestService
        {
            void DoSomething();
        }

        public class TestServiceImpl : ITestService
        {
            public void DoSomething() { }
        }

        private class UnknownServiceDefinition : ServiceDefinition
        {
            public UnknownServiceDefinition() : base(typeof(object), Lifetime.Transient) { }
        }

        private class TestInterceptorFactory : InterceptorFactory
        {
            public override IInterceptor CreateInstance(IServiceProvider serviceProvider)
            {
                return new TestInterceptor();
            }
        }

        private class TestInterceptor : AbstractInterceptorAttribute
        {
            public override Task Invoke(AspectContext context, AspectDelegate next)
            {
                return next(context);
            }
        }
    }
}
