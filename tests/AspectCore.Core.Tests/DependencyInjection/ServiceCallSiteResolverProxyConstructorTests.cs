using System;
using System.Collections.Generic;
using System.Linq;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DependencyInjection
{
    public class ServiceCallSiteResolverProxyConstructorTests
    {
        [Fact]
        public void Resolve_ProxyServiceWithThreeParameterConstructor_ReturnsInstance()
        {
            // Tests lines 115-121: proxy constructor with (IAspectActivatorFactory, IServiceProvider, serviceType)
            var context = new ServiceContext();
            // Register the inner service so it can be resolved
            context.AddType<ICallSiteTest, CallSiteTestImpl>(Lifetime.Transient);
            var table = new ServiceTable(context);
            var callSiteResolver = new ServiceCallSiteResolver(table);

            // Create a proxy service definition where the proxy type has a 3-parameter constructor
            var proxyDef = new ProxyServiceDefinition(
                new TypeServiceDefinition(typeof(ICallSiteTest), typeof(CallSiteTestImpl), Lifetime.Transient),
                typeof(CallSiteTestProxy));

            var resolver = new ServiceResolver(context);
            var result = callSiteResolver.Resolve(proxyDef)(resolver);
            Assert.NotNull(result);
            Assert.IsType<CallSiteTestProxy>(result);
        }

        [Fact]
        public void Resolve_ProxyServiceWithTwoParameterConstructor_ReturnsInstance()
        {
            // Tests the legacy 2-parameter constructor path (lines 125-134)
            var context = new ServiceContext();
            context.AddType<ICallSiteTest, CallSiteTestImpl>(Lifetime.Transient);
            var table = new ServiceTable(context);
            var callSiteResolver = new ServiceCallSiteResolver(table);

            var proxyDef = new ProxyServiceDefinition(
                new TypeServiceDefinition(typeof(ICallSiteTest), typeof(CallSiteTestImpl), Lifetime.Transient),
                typeof(CallSiteTestProxyTwoParam));

            var resolver = new ServiceResolver(context);
            var result = callSiteResolver.Resolve(proxyDef)(resolver);
            Assert.NotNull(result);
            Assert.IsType<CallSiteTestProxyTwoParam>(result);
        }

        [Fact]
        public void Resolve_ProxyServiceWithNoSuitableConstructor_Throws()
        {
            // Tests line 136: throw InvalidOperationException when no suitable constructor
            var context = new ServiceContext();
            context.AddType<ICallSiteTest, CallSiteTestImpl>(Lifetime.Transient);
            var table = new ServiceTable(context);
            var callSiteResolver = new ServiceCallSiteResolver(table);

            var proxyDef = new ProxyServiceDefinition(
                new TypeServiceDefinition(typeof(ICallSiteTest), typeof(CallSiteTestImpl), Lifetime.Transient),
                typeof(CallSiteTestProxyNoCtor));

            var resolver = new ServiceResolver(context);
            Assert.Throws<InvalidOperationException>(() => callSiteResolver.Resolve(proxyDef)(resolver));
        }

        [Fact]
        public void Resolve_ProxyServiceWithClassServiceType_ResolvesViaTypeService()
        {
            // Tests lines 107-109: when ServiceType is a class, resolves via type service
            // The proxy type needs a constructor that can be resolved from the container
            var context = new ServiceContext();
            // Register the class service type so it can be resolved
            context.AddType<CallSiteTestClass, CallSiteTestClass>(Lifetime.Transient);
            var table = new ServiceTable(context);
            var callSiteResolver = new ServiceCallSiteResolver(table);

            // Use a proxy type with a parameterless constructor so it can be resolved
            var proxyDef = new ProxyServiceDefinition(
                new TypeServiceDefinition(typeof(CallSiteTestClass), typeof(CallSiteTestClass), Lifetime.Transient),
                typeof(CallSiteTestClassProxySimple));

            var resolver = new ServiceResolver(context);
            var result = callSiteResolver.Resolve(proxyDef)(resolver);
            Assert.NotNull(result);
        }

        [Fact]
        public void Resolve_ProxyService_ResolvesInnerServiceDefinition()
        {
            // Tests the serviceResolver = Resolve(proxyServiceDefinition.ServiceDefinition) path
            var context = new ServiceContext();
            context.AddType<ICallSiteTest, CallSiteTestImpl>(Lifetime.Transient);
            var table = new ServiceTable(context);
            var callSiteResolver = new ServiceCallSiteResolver(table);

            var proxyDef = new ProxyServiceDefinition(
                new InstanceServiceDefinition(typeof(ICallSiteTest), new CallSiteTestImpl()),
                typeof(CallSiteTestProxy));

            var resolver = new ServiceResolver(context);
            var result = callSiteResolver.Resolve(proxyDef)(resolver);
            Assert.NotNull(result);
            Assert.IsType<CallSiteTestProxy>(result);
        }

        #region Test Types

        public interface ICallSiteTest
        {
            int GetValue();
        }

        public class CallSiteTestImpl : ICallSiteTest
        {
            public int GetValue() => 42;
        }

        public class CallSiteTestClass
        {
            public CallSiteTestClass() { }
            public virtual int GetValue() => 42;
        }

        // Class proxy with constructor that takes resolvable dependencies
        public class CallSiteTestClassProxy : CallSiteTestClass
        {
            public CallSiteTestClassProxy(CallSiteTestClass inner) { }
            public override int GetValue() => 99;
        }

        // Class proxy with parameterless constructor for simple resolution
        public class CallSiteTestClassProxySimple : CallSiteTestClass
        {
            public CallSiteTestClassProxySimple() { }
            public override int GetValue() => 99;
        }

        // Proxy with 3-parameter constructor: (IAspectActivatorFactory, IServiceProvider, serviceType)
        public class CallSiteTestProxy : ICallSiteTest
        {
            private readonly IAspectActivatorFactory _factory;
            private readonly IServiceProvider _provider;
            private readonly ICallSiteTest _impl;

            public CallSiteTestProxy(IAspectActivatorFactory factory, IServiceProvider provider, ICallSiteTest impl)
            {
                _factory = factory;
                _provider = provider;
                _impl = impl;
            }

            public int GetValue() => _impl?.GetValue() ?? 0;
        }

        // Proxy with 2-parameter constructor: (IAspectActivatorFactory, serviceType)
        public class CallSiteTestProxyTwoParam : ICallSiteTest
        {
            private readonly IAspectActivatorFactory _factory;
            private readonly ICallSiteTest _impl;

            public CallSiteTestProxyTwoParam(IAspectActivatorFactory factory, ICallSiteTest impl)
            {
                _factory = factory;
                _impl = impl;
            }

            public int GetValue() => _impl?.GetValue() ?? 0;
        }

        // Proxy with no suitable constructor (only parameterless)
        public class CallSiteTestProxyNoCtor : ICallSiteTest
        {
            public CallSiteTestProxyNoCtor() { }
            public int GetValue() => 0;
        }

        #endregion
    }
}
