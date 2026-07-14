using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class InterceptorCollectorTests
    {
        [Fact]
        public void Constructor_WithNullInterceptorSelectors_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new InterceptorCollector(null, new IAdditionalInterceptorSelector[0], new FakePropertyInjectorFactory(), new AspectCachingProvider()));
            Assert.Equal("interceptorSelectors", ex.ParamName);
        }

        [Fact]
        public void Constructor_WithNullAdditionalInterceptorSelectors_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new InterceptorCollector(new IInterceptorSelector[0], null, new FakePropertyInjectorFactory(), new AspectCachingProvider()));
            Assert.Equal("additionalInterceptorSelectors", ex.ParamName);
        }

        [Fact]
        public void Constructor_WithNullPropertyInjectorFactory_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new InterceptorCollector(new IInterceptorSelector[0], new IAdditionalInterceptorSelector[0], null, new AspectCachingProvider()));
            Assert.Equal("propertyInjectorFactory", ex.ParamName);
        }

        [Fact]
        public void Constructor_WithNullAspectCachingProvider_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new InterceptorCollector(new IInterceptorSelector[0], new IAdditionalInterceptorSelector[0], new FakePropertyInjectorFactory(), null));
            Assert.Equal("aspectCachingProvider", ex.ParamName);
        }

        [Fact]
        public void Collect_WithNullServiceMethod_ThrowsArgumentNullException()
        {
            var collector = CreateCollector();
            var method = typeof(ITestService).GetMethod(nameof(ITestService.DoSomething));
            var ex = Assert.Throws<ArgumentNullException>(() => collector.Collect(null, method, method));
            Assert.Equal("serviceMethod", ex.ParamName);
        }

        [Fact]
        public void Collect_WithNullImplementationMethod_ThrowsArgumentNullException()
        {
            var collector = CreateCollector();
            var method = typeof(ITestService).GetMethod(nameof(ITestService.DoSomething));
            var ex = Assert.Throws<ArgumentNullException>(() => collector.Collect(method, null, method));
            Assert.Equal("implementationMethod", ex.ParamName);
        }

        [Fact]
        public void Collect_ReturnsInterceptorsFromSelector()
        {
            var interceptor = new FakeInterceptor();
            var selector = new FakeInterceptorSelector(interceptor);
            var collector = new InterceptorCollector(
                new IInterceptorSelector[] { selector },
                new IAdditionalInterceptorSelector[0],
                new FakePropertyInjectorFactory(),
                new AspectCachingProvider());

            var method = typeof(TestServiceImpl).GetMethod(nameof(ITestService.DoSomething));
            var result = collector.Collect(method, method, method);
            Assert.Single(result);
            Assert.Same(interceptor, result.First());
        }

        [Fact]
        public void Collect_ReturnsInterceptorsFromAdditionalSelector()
        {
            var interceptor = new FakeInterceptor();
            var selector = new FakeAdditionalInterceptorSelector(interceptor);
            var collector = new InterceptorCollector(
                new IInterceptorSelector[0],
                new IAdditionalInterceptorSelector[] { selector },
                new FakePropertyInjectorFactory(),
                new AspectCachingProvider());

            var method = typeof(TestServiceImpl).GetMethod(nameof(ITestService.DoSomething));
            var result = collector.Collect(method, method, method);
            Assert.Single(result);
            Assert.Same(interceptor, result.First());
        }

        [Fact]
        public void Collect_DeduplicatesSelectors()
        {
            var interceptor = new FakeInterceptor();
            var selector = new FakeInterceptorSelector(interceptor);
            var collector = new InterceptorCollector(
                new IInterceptorSelector[] { selector, selector },
                new IAdditionalInterceptorSelector[0],
                new FakePropertyInjectorFactory(),
                new AspectCachingProvider());

            var method = typeof(TestServiceImpl).GetMethod(nameof(ITestService.DoSomething));
            var result = collector.Collect(method, method, method);
            Assert.Single(result);
        }

        [Fact]
        public void Collect_HandlesMultipleInterceptorsWithAllowMultiple()
        {
            var interceptor1 = new FakeInterceptor { AllowMultiple = true };
            var interceptor2 = new FakeInterceptor { AllowMultiple = true };
            var selector = new FakeInterceptorSelector(interceptor1, interceptor2);
            var collector = new InterceptorCollector(
                new IInterceptorSelector[] { selector },
                new IAdditionalInterceptorSelector[0],
                new FakePropertyInjectorFactory(),
                new AspectCachingProvider());

            var method = typeof(TestServiceImpl).GetMethod(nameof(ITestService.DoSomething));
            var result = collector.Collect(method, method, method);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void Collect_DeduplicatesNonAllowMultipleInterceptors()
        {
            var interceptor1 = new FakeInterceptor { AllowMultiple = false };
            var interceptor2 = new FakeInterceptor { AllowMultiple = false };
            var selector = new FakeInterceptorSelector(interceptor1, interceptor2);
            var collector = new InterceptorCollector(
                new IInterceptorSelector[] { selector },
                new IAdditionalInterceptorSelector[0],
                new FakePropertyInjectorFactory(),
                new AspectCachingProvider());

            var method = typeof(TestServiceImpl).GetMethod(nameof(ITestService.DoSomething));
            var result = collector.Collect(method, method, method);
            Assert.Single(result);
        }

        [Fact]
        public void Collect_SortsByOrder()
        {
            var interceptor1 = new FakeInterceptor { Order = 2, AllowMultiple = true };
            var interceptor2 = new FakeInterceptor { Order = 1, AllowMultiple = true };
            var selector = new FakeInterceptorSelector(interceptor1, interceptor2);
            var collector = new InterceptorCollector(
                new IInterceptorSelector[] { selector },
                new IAdditionalInterceptorSelector[0],
                new FakePropertyInjectorFactory(),
                new AspectCachingProvider());

            var method = typeof(TestServiceImpl).GetMethod(nameof(ITestService.DoSomething));
            var result = collector.Collect(method, method, method).ToList();
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Order);
            Assert.Equal(2, result[1].Order);
        }

        [Fact]
        public void Collect_CachesResult()
        {
            var interceptor = new FakeInterceptor();
            var selector = new FakeInterceptorSelector(interceptor);
            var collector = new InterceptorCollector(
                new IInterceptorSelector[] { selector },
                new IAdditionalInterceptorSelector[0],
                new FakePropertyInjectorFactory(),
                new AspectCachingProvider());

            var method = typeof(TestServiceImpl).GetMethod(nameof(ITestService.DoSomething));
            var result1 = collector.Collect(method, method, method);
            var result2 = collector.Collect(method, method, method);
            Assert.Same(result1, result2);
        }

        [Fact]
        public void Collect_WithInheritedInterceptorFromInterface()
        {
            var interceptor = new FakeInterceptor { Inherited = true };
            var selector = new FakeInterceptorSelector(interceptor);
            var collector = new InterceptorCollector(
                new IInterceptorSelector[] { selector },
                new IAdditionalInterceptorSelector[0],
                new FakePropertyInjectorFactory(),
                new AspectCachingProvider());

            var interfaceMethod = typeof(ITestService).GetMethod(nameof(ITestService.DoSomething));
            var implMethod = typeof(TestServiceImpl).GetMethod(nameof(TestServiceImpl.DoSomething));
            var result = collector.Collect(interfaceMethod, implMethod, implMethod);
            Assert.Single(result);
        }

        private static InterceptorCollector CreateCollector()
        {
            return new InterceptorCollector(
                new IInterceptorSelector[0],
                new IAdditionalInterceptorSelector[0],
                new FakePropertyInjectorFactory(),
                new AspectCachingProvider());
        }

        public interface ITestService
        {
            void DoSomething();
        }

        public class TestServiceImpl : ITestService
        {
            public void DoSomething() { }
        }

        private class FakeInterceptor : IInterceptor
        {
            public bool AllowMultiple { get; set; }
            public bool Inherited { get; set; }
            public int Order { get; set; }
            public Task Invoke(AspectContext context, AspectDelegate next) => next(context);
        }

        private class FakeInterceptorSelector : IInterceptorSelector
        {
            private readonly IInterceptor[] _interceptors;
            public FakeInterceptorSelector(params IInterceptor[] interceptors) => _interceptors = interceptors;
            public IEnumerable<IInterceptor> Select(MethodInfo method) => _interceptors;
        }

        private class FakeAdditionalInterceptorSelector : IAdditionalInterceptorSelector
        {
            private readonly IInterceptor[] _interceptors;
            public FakeAdditionalInterceptorSelector(params IInterceptor[] interceptors) => _interceptors = interceptors;
            public IEnumerable<IInterceptor> Select(MethodInfo serviceMethod, MethodInfo implementationMethod) => _interceptors;
        }

        private class FakePropertyInjectorFactory : IPropertyInjectorFactory
        {
            public IPropertyInjector Create(Type implementationType) => new FakePropertyInjector();
        }

        private class FakePropertyInjector : IPropertyInjector
        {
            public void Invoke(object implementation) { }
        }
    }
}
