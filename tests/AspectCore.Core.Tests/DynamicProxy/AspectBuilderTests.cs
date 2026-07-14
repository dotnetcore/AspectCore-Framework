using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class AspectBuilderTests
    {
        private static AspectDelegate CompleteDelegate => context =>
        {
            context.ReturnValue = "complete";
            return Task.CompletedTask;
        };

        // ---------- Constructor ----------

        [Fact]
        public void Constructor_NullComplete_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AspectBuilder(null, null));
        }

        [Fact]
        public void Constructor_NullDelegates_UsesEmptyList()
        {
            var builder = new AspectBuilder(CompleteDelegate, null);

            Assert.NotNull(builder.Delegates);
            Assert.Empty(builder.Delegates);
        }

        [Fact]
        public void Constructor_WithDelegates_StoresThem()
        {
            var delegates = new List<Func<AspectDelegate, AspectDelegate>>
            {
                next => context => next(context),
                next => context => next(context),
            };

            var builder = new AspectBuilder(CompleteDelegate, delegates);

            Assert.Equal(2, builder.Delegates.Count());
        }

        // ---------- Delegates ----------

        [Fact]
        public void Delegates_ReturnsStoredDelegates()
        {
            var delegates = new List<Func<AspectDelegate, AspectDelegate>>
            {
                next => context => next(context),
            };

            var builder = new AspectBuilder(CompleteDelegate, delegates);

            Assert.Same(delegates, builder.Delegates);
        }

        // ---------- AddAspectDelegate ----------

        [Fact]
        public void AddAspectDelegate_NullInvoke_ThrowsArgumentNullException()
        {
            var builder = new AspectBuilder(CompleteDelegate, null);

            Assert.Throws<ArgumentNullException>(() => builder.AddAspectDelegate(null));
        }

        [Fact]
        public void AddAspectDelegate_ValidInvoke_AddsToDelegates()
        {
            var builder = new AspectBuilder(CompleteDelegate, null);

            builder.AddAspectDelegate((context, next) => next(context));

            Assert.Single(builder.Delegates);
        }

        [Fact]
        public void AddAspectDelegate_MultipleCalls_AppendsInOrder()
        {
            var builder = new AspectBuilder(CompleteDelegate, null);

            builder.AddAspectDelegate((context, next) => next(context));
            builder.AddAspectDelegate((context, next) => next(context));
            builder.AddAspectDelegate((context, next) => next(context));

            Assert.Equal(3, builder.Delegates.Count());
        }

        // ---------- Build ----------

        [Fact]
        public void Build_EmptyDelegates_ReturnsCompleteDelegate()
        {
            var builder = new AspectBuilder(CompleteDelegate, null);

            var aspectDelegate = builder.Build();

            Assert.NotNull(aspectDelegate);
            // The returned delegate should invoke _complete when called.
            var context = CreateContext();
            aspectDelegate(context);
            Assert.Equal("complete", context.ReturnValue);
        }

        [Fact]
        public void Build_SingleDelegate_WrapsComplete()
        {
            var builder = new AspectBuilder(CompleteDelegate, null);
            builder.AddAspectDelegate((context, next) =>
            {
                context.ReturnValue = "before";
                return next(context);
            });

            var aspectDelegate = builder.Build();
            var context = CreateContext();
            aspectDelegate(context);

            // The interceptor sets "before", then calls next (complete) which sets "complete".
            Assert.Equal("complete", context.ReturnValue);
        }

        [Fact]
        public void Build_MultipleDelegates_ExecutesInOrder()
        {
            var executionOrder = new List<string>();
            var builder = new AspectBuilder(CompleteDelegate, null);

            builder.AddAspectDelegate((context, next) =>
            {
                executionOrder.Add("first");
                return next(context);
            });
            builder.AddAspectDelegate((context, next) =>
            {
                executionOrder.Add("second");
                return next(context);
            });
            builder.AddAspectDelegate((context, next) =>
            {
                executionOrder.Add("third");
                return next(context);
            });

            var aspectDelegate = builder.Build();
            var context = CreateContext();
            aspectDelegate(context);

            Assert.Equal(new[] { "first", "second", "third" }, executionOrder);
            Assert.Equal("complete", context.ReturnValue);
        }

        [Fact]
        public void Build_DelegateCanShortCircuit_WithoutCallingNext()
        {
            var builder = new AspectBuilder(CompleteDelegate, null);
            var completeCalled = false;
            var complete = (AspectDelegate)(context =>
            {
                completeCalled = true;
                return Task.CompletedTask;
            });
            var builder2 = new AspectBuilder(complete, null);

            builder2.AddAspectDelegate((context, next) =>
            {
                context.ReturnValue = "short-circuit";
                // Do not call next - short circuit the pipeline.
                return Task.CompletedTask;
            });

            var aspectDelegate = builder2.Build();
            var context = CreateContext();
            aspectDelegate(context);

            Assert.Equal("short-circuit", context.ReturnValue);
            Assert.False(completeCalled);
        }

        [Fact]
        public void Build_CachesResult_ReturnsSameInstance()
        {
            var builder = new AspectBuilder(CompleteDelegate, null);
            builder.AddAspectDelegate((context, next) => next(context));

            var first = builder.Build();
            var second = builder.Build();

            Assert.Same(first, second);
        }

        [Fact]
        public void Build_CachedDelegate_ProducesSameBehavior()
        {
            var builder = new AspectBuilder(CompleteDelegate, null);
            builder.AddAspectDelegate((context, next) => next(context));

            var aspectDelegate = builder.Build();

            var context1 = CreateContext();
            var context2 = CreateContext();
            aspectDelegate(context1);
            aspectDelegate(context2);

            Assert.Equal("complete", context1.ReturnValue);
            Assert.Equal("complete", context2.ReturnValue);
        }

        [Fact]
        public void Build_WithProvidedDelegatesList_ComposesThem()
        {
            var executionOrder = new List<string>();
            var delegates = new List<Func<AspectDelegate, AspectDelegate>>
            {
                next => context =>
                {
                    executionOrder.Add("a");
                    return next(context);
                },
                next => context =>
                {
                    executionOrder.Add("b");
                    return next(context);
                },
            };

            var builder = new AspectBuilder(CompleteDelegate, delegates);
            var aspectDelegate = builder.Build();
            var context = CreateContext();
            aspectDelegate(context);

            Assert.Equal(new[] { "a", "b" }, executionOrder);
        }

        // ---------- Helpers ----------

        private static TestAspectContext CreateContext()
        {
            return new TestAspectContext();
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
