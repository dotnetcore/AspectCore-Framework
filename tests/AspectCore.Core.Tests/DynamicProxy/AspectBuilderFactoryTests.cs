using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class AspectBuilderFactoryTests
    {
        // ---------- Test types ----------

        public interface ITestService
        {
            void DoSomething();
            int GetValue();
            string GetName();
        }

        public class TestService : ITestService
        {
            public virtual void DoSomething() { }

            public virtual int GetValue() => 42;

            public virtual string GetName() => "test";
        }

        public interface IAnotherService
        {
            void Run();
        }

        public class AnotherService : IAnotherService
        {
            public virtual void Run() { }
        }

        // ---------- Constructor ----------

        [Fact]
        public void Constructor_NullInterceptorCollector_ThrowsArgumentNullException()
        {
            var provider = new AspectCachingProvider();

            Assert.Throws<ArgumentNullException>(() =>
                new AspectBuilderFactory(null, provider));
        }

        [Fact]
        public void Constructor_NullAspectCachingProvider_ThrowsArgumentNullException()
        {
            var collector = new FakeInterceptorCollector();

            Assert.Throws<ArgumentNullException>(() =>
                new AspectBuilderFactory(collector, null));
        }

        // ---------- GetBuilder(MethodInfo, MethodInfo) ----------

        [Fact]
        public void GetBuilder_TwoArgs_NullServiceMethod_ThrowsArgumentNullException()
        {
            var factory = CreateFactory();
            var implMethod = GetMethod<TestService>(nameof(TestService.GetValue));

            Assert.Throws<ArgumentNullException>(() =>
                factory.GetBuilder(null, implMethod));
        }

        [Fact]
        public void GetBuilder_TwoArgs_NullImplementationMethod_ThrowsArgumentNullException()
        {
            var factory = CreateFactory();
            var serviceMethod = GetMethod<ITestService>(nameof(ITestService.GetValue));

            Assert.Throws<ArgumentNullException>(() =>
                factory.GetBuilder(serviceMethod, null));
        }

        [Fact]
        public void GetBuilder_TwoArgs_ReturnsNonNullBuilder()
        {
            var factory = CreateFactory();
            var serviceMethod = GetMethod<ITestService>(nameof(ITestService.GetValue));
            var implMethod = GetMethod<TestService>(nameof(TestService.GetValue));

            var builder = factory.GetBuilder(serviceMethod, implMethod);

            Assert.NotNull(builder);
            Assert.IsAssignableFrom<IAspectBuilder>(builder);
        }

        // ---------- GetBuilder(MethodInfo, MethodInfo, MethodInfo) ----------

        [Fact]
        public void GetBuilder_ThreeArgs_NullServiceMethod_ThrowsArgumentNullException()
        {
            var factory = CreateFactory();
            var implMethod = GetMethod<TestService>(nameof(TestService.GetValue));

            Assert.Throws<ArgumentNullException>(() =>
                factory.GetBuilder(null, implMethod, implMethod));
        }

        [Fact]
        public void GetBuilder_ThreeArgs_NullImplementationMethod_ThrowsArgumentNullException()
        {
            var factory = CreateFactory();
            var serviceMethod = GetMethod<ITestService>(nameof(ITestService.GetValue));

            Assert.Throws<ArgumentNullException>(() =>
                factory.GetBuilder(serviceMethod, null, serviceMethod));
        }

        [Fact]
        public void GetBuilder_ThreeArgs_NullPredicateMethod_DoesNotThrow()
        {
            var factory = CreateFactory();
            var serviceMethod = GetMethod<ITestService>(nameof(ITestService.GetValue));
            var implMethod = GetMethod<TestService>(nameof(TestService.GetValue));

            // predicateMethod is not validated for null in the three-arg overload.
            var builder = factory.GetBuilder(serviceMethod, implMethod, null);

            Assert.NotNull(builder);
        }

        [Fact]
        public void GetBuilder_ThreeArgs_ReturnsNonNullBuilder()
        {
            var factory = CreateFactory();
            var serviceMethod = GetMethod<ITestService>(nameof(ITestService.GetValue));
            var implMethod = GetMethod<TestService>(nameof(TestService.GetValue));

            var builder = factory.GetBuilder(serviceMethod, implMethod, serviceMethod);

            Assert.NotNull(builder);
            Assert.IsAssignableFrom<IAspectBuilder>(builder);
        }

        // ---------- Caching behavior ----------

        [Fact]
        public void GetBuilder_SameInputs_ReturnsSameInstance()
        {
            var factory = CreateFactory();
            var serviceMethod = GetMethod<ITestService>(nameof(ITestService.GetValue));
            var implMethod = GetMethod<TestService>(nameof(TestService.GetValue));

            var first = factory.GetBuilder(serviceMethod, implMethod, serviceMethod);
            var second = factory.GetBuilder(serviceMethod, implMethod, serviceMethod);

            Assert.Same(first, second);
        }

        [Fact]
        public void GetBuilder_DifferentInputs_ReturnsDifferentInstances()
        {
            var factory = CreateFactory();
            var serviceMethod1 = GetMethod<ITestService>(nameof(ITestService.GetValue));
            var implMethod1 = GetMethod<TestService>(nameof(TestService.GetValue));
            var serviceMethod2 = GetMethod<ITestService>(nameof(ITestService.GetName));
            var implMethod2 = GetMethod<TestService>(nameof(TestService.GetName));

            var first = factory.GetBuilder(serviceMethod1, implMethod1, serviceMethod1);
            var second = factory.GetBuilder(serviceMethod2, implMethod2, serviceMethod2);

            Assert.NotSame(first, second);
        }

        [Fact]
        public void GetBuilder_TwoArgs_UsesServiceMethodAsPredicate()
        {
            // The two-arg overload delegates to the three-arg overload using
            // serviceMethod as the predicate method, so they share the same cache key.
            var factory = CreateFactory();
            var serviceMethod = GetMethod<ITestService>(nameof(ITestService.GetValue));
            var implMethod = GetMethod<TestService>(nameof(TestService.GetValue));

            var twoArgResult = factory.GetBuilder(serviceMethod, implMethod);
            var threeArgResult = factory.GetBuilder(serviceMethod, implMethod, serviceMethod);

            Assert.Same(twoArgResult, threeArgResult);
        }

        [Fact]
        public void GetBuilder_DifferentPredicate_ReturnsDifferentInstance()
        {
            var factory = CreateFactory();
            var serviceMethod = GetMethod<ITestService>(nameof(ITestService.GetValue));
            var implMethod = GetMethod<TestService>(nameof(TestService.GetValue));
            var predicateMethod = GetMethod<ITestService>(nameof(ITestService.GetName));

            var withServicePredicate = factory.GetBuilder(serviceMethod, implMethod, serviceMethod);
            var withNamePredicate = factory.GetBuilder(serviceMethod, implMethod, predicateMethod);

            Assert.NotSame(withServicePredicate, withNamePredicate);
        }

        // ---------- Interceptor collection ----------

        [Fact]
        public void GetBuilder_CollectsInterceptors_FromInterceptorCollector()
        {
            var collector = new FakeInterceptorCollector();
            collector.Interceptors.Add(new RecordingInterceptor("A"));
            collector.Interceptors.Add(new RecordingInterceptor("B"));
            var factory = CreateFactory(collector);

            var serviceMethod = GetMethod<ITestService>(nameof(ITestService.GetValue));
            var implMethod = GetMethod<TestService>(nameof(TestService.GetValue));

            var builder = factory.GetBuilder(serviceMethod, implMethod, serviceMethod);

            // The builder should have one delegate per interceptor.
            Assert.Equal(2, builder.Delegates.Count());
        }

        [Fact]
        public void GetBuilder_NoInterceptors_BuilderHasEmptyDelegates()
        {
            var collector = new FakeInterceptorCollector();
            var factory = CreateFactory(collector);

            var serviceMethod = GetMethod<ITestService>(nameof(ITestService.GetValue));
            var implMethod = GetMethod<TestService>(nameof(TestService.GetValue));

            var builder = factory.GetBuilder(serviceMethod, implMethod, serviceMethod);

            Assert.Empty(builder.Delegates);
        }

        [Fact]
        public void GetBuilder_Interceptors_AreExecutedInOrder()
        {
            var executionOrder = new List<string>();
            var collector = new FakeInterceptorCollector();
            collector.Interceptors.Add(new CallbackInterceptor(context =>
            {
                executionOrder.Add("A");
                return Task.CompletedTask;
            }));
            collector.Interceptors.Add(new CallbackInterceptor(context =>
            {
                executionOrder.Add("B");
                return Task.CompletedTask;
            }));
            var factory = CreateFactory(collector);

            var serviceMethod = GetMethod<ITestService>(nameof(ITestService.GetValue));
            var implMethod = GetMethod<TestService>(nameof(TestService.GetValue));

            var builder = factory.GetBuilder(serviceMethod, implMethod, serviceMethod);
            var aspectDelegate = builder.Build();
            var testContext = new TestAspectContext();
            aspectDelegate(testContext);

            Assert.Equal(new[] { "A", "B" }, executionOrder);
        }

        [Fact]
        public void GetBuilder_CollectorReceivesCorrectMethodArguments()
        {
            var collector = new FakeInterceptorCollector();
            var factory = CreateFactory(collector);

            var serviceMethod = GetMethod<ITestService>(nameof(ITestService.GetValue));
            var implMethod = GetMethod<TestService>(nameof(TestService.GetValue));
            var predicateMethod = GetMethod<ITestService>(nameof(ITestService.GetName));

            factory.GetBuilder(serviceMethod, implMethod, predicateMethod);

            Assert.NotNull(collector.LastCollected);
            Assert.Same(serviceMethod, collector.LastCollected.ServiceMethod);
            Assert.Same(implMethod, collector.LastCollected.ImplementationMethod);
            Assert.Same(predicateMethod, collector.LastCollected.PredicateMethod);
        }

        // ---------- Create(AspectContext) ----------

        [Fact]
        public void Create_NullContext_ThrowsArgumentNullException()
        {
            var factory = CreateFactory();

            Assert.Throws<ArgumentNullException>(() => factory.Create(null));
        }

        [Fact]
        public void Create_ValidContext_ReturnsBuilder()
        {
            var factory = CreateFactory();
            var method = GetMethod<TestService>(nameof(TestService.GetValue));
            var context = new RuntimeAspectContext(
                null, method, method, method, method,
                new TestService(), new TestService(),
                Array.Empty<object>());

            var builder = factory.Create(context);

            Assert.NotNull(builder);
            Assert.IsAssignableFrom<IAspectBuilder>(builder);
        }

        [Fact]
        public void Create_UsesContextMethods_AsCacheKey()
        {
            var factory = CreateFactory();
            var method = GetMethod<TestService>(nameof(TestService.GetValue));
            var context = new RuntimeAspectContext(
                null, method, method, method, method,
                new TestService(), new TestService(),
                Array.Empty<object>());

            var fromCreate = factory.Create(context);
            var fromGetBuilder = factory.GetBuilder(method, method, method);

            // Create delegates to GetBuilder with the context's methods, so they
            // should share the same cache entry.
            Assert.Same(fromCreate, fromGetBuilder);
        }

        // ---------- Helpers ----------

        private static AspectBuilderFactory CreateFactory(IInterceptorCollector collector = null)
        {
            var provider = new AspectCachingProvider();
            return new AspectBuilderFactory(collector ?? new FakeInterceptorCollector(), provider);
        }

        private static MethodInfo GetMethod<T>(string name)
        {
            return typeof(T).GetMethod(name);
        }

        private class FakeInterceptorCollector : IInterceptorCollector
        {
            public List<IInterceptor> Interceptors { get; } = new List<IInterceptor>();

            public CollectionRecord LastCollected { get; private set; }

            public IEnumerable<IInterceptor> Collect(MethodInfo serviceMethod, MethodInfo implementationMethod, MethodInfo predicateMethod)
            {
                LastCollected = new CollectionRecord
                {
                    ServiceMethod = serviceMethod,
                    ImplementationMethod = implementationMethod,
                    PredicateMethod = predicateMethod,
                };
                return Interceptors;
            }

            public class CollectionRecord
            {
                public MethodInfo ServiceMethod { get; set; }
                public MethodInfo ImplementationMethod { get; set; }
                public MethodInfo PredicateMethod { get; set; }
            }
        }

        private class RecordingInterceptor : IInterceptor
        {
            private readonly string _name;

            public RecordingInterceptor(string name)
            {
                _name = name;
            }

            public bool AllowMultiple => true;

            public bool Inherited { get; set; }

            public int Order { get; set; }

            public Task Invoke(AspectContext context, AspectDelegate next)
            {
                return next(context);
            }
        }

        private class CallbackInterceptor : IInterceptor
        {
            private readonly Func<AspectContext, Task> _callback;

            public CallbackInterceptor(Func<AspectContext, Task> callback)
            {
                _callback = callback;
            }

            public bool AllowMultiple => true;

            public bool Inherited { get; set; }

            public int Order { get; set; }

            public async Task Invoke(AspectContext context, AspectDelegate next)
            {
                await _callback(context);
                await next(context);
            }
        }

        private class TestAspectContext : AspectContext
        {
            private IDictionary<string, object> _additionalData;

            public override IDictionary<string, object> AdditionalData =>
                _additionalData ?? (_additionalData = new Dictionary<string, object>());

            public override object ReturnValue { get; set; }

            public override IServiceProvider ServiceProvider => null;

            public override MethodInfo ServiceMethod => null;

            public override object Implementation => null;

            public override MethodInfo ImplementationMethod => null;

            public override object[] Parameters => null;

            public override MethodInfo ProxyMethod => null;

            public override MethodInfo PredicateMethod => null;

            public override object Proxy => null;

            public override Task Break() => Task.CompletedTask;

            public override Task Invoke(AspectDelegate next) => next(this);

            public override Task Complete()
            {
                ReturnValue = "complete";
                return Task.CompletedTask;
            }
        }
    }
}
