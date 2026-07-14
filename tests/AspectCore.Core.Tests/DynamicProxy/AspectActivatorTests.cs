using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class AspectActivatorTests
    {
        private static AspectActivator CreateActivator(
            object returnValue = null,
            Task invokeTask = null,
            bool throwAspectException = true,
            Action<AspectContext> onRelease = null)
        {
            var config = new AspectConfiguration { ThrowAspectException = throwAspectException };
            var contextFactory = new FakeAspectContextFactory(returnValue, onRelease);
            var builderFactory = new FakeAspectBuilderFactory(invokeTask ?? Task.CompletedTask);
            return new AspectActivator(contextFactory, builderFactory, config);
        }

        private static AspectActivatorContext CreateActivatorContext()
        {
            var method = typeof(TestService).GetMethod(nameof(TestService.DoSomething));
            return new AspectActivatorContext(method, method, method, method, new TestService(), null, new object[0]);
        }

        [Fact]
        public void Invoke_WithFaultedTask_ThrowsWrappedException()
        {
            var faultedTask = Task.FromException(new InvalidOperationException("test error"));
            var activator = CreateActivator(invokeTask: faultedTask, throwAspectException: true);
            var context = CreateActivatorContext();
            var ex = Assert.Throws<AspectInvocationException>(() => activator.Invoke<object>(context));
            Assert.IsType<InvalidOperationException>(ex.InnerException);
        }

        [Fact]
        public void Invoke_WithFaultedTaskAndNoThrowAspect_ReThrowsOriginalException()
        {
            var faultedTask = Task.FromException(new InvalidOperationException("test error"));
            var activator = CreateActivator(invokeTask: faultedTask, throwAspectException: false);
            var context = CreateActivatorContext();
            var ex = Assert.Throws<InvalidOperationException>(() => activator.Invoke<object>(context));
            Assert.Equal("test error", ex.Message);
        }

        [Fact]
        public void Invoke_WithCompletedTask_ReturnsValue()
        {
            var activator = CreateActivator(returnValue: "hello", invokeTask: Task.CompletedTask);
            var context = CreateActivatorContext();
            var result = activator.Invoke<string>(context);
            Assert.Equal("hello", result);
        }

        [Fact]
        public void Invoke_WithRunningTask_WaitsForCompletion()
        {
            var runningTask = Task.Run(() => System.Threading.Thread.Sleep(50));
            var activator = CreateActivator(returnValue: 42, invokeTask: runningTask);
            var context = CreateActivatorContext();
            var result = activator.Invoke<int>(context);
            Assert.Equal(42, result);
        }

        [Fact]
        public async Task InvokeTask_WithNullReturnValue_ReturnsDefault()
        {
            var activator = CreateActivator(returnValue: null, invokeTask: Task.CompletedTask);
            var context = CreateActivatorContext();
            var result = await activator.InvokeTask<int>(context);
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task InvokeTask_WithTaskReturnValue_AwaitsAndReturns()
        {
            var innerTask = Task.FromResult(42);
            var activator = CreateActivator(returnValue: innerTask, invokeTask: Task.CompletedTask);
            var context = CreateActivatorContext();
            var result = await activator.InvokeTask<int>(context);
            Assert.Equal(42, result);
        }

        [Fact]
        public async Task InvokeTask_WithNonGenericTaskReturnValue_AwaitsAndReturnsDefault()
        {
            var innerTask = Task.CompletedTask;
            var activator = CreateActivator(returnValue: innerTask, invokeTask: Task.CompletedTask);
            var context = CreateActivatorContext();
            var result = await activator.InvokeTask<int>(context);
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task InvokeTask_WithInvalidReturnValue_ThrowsAspectInvalidCastException()
        {
            var activator = CreateActivator(returnValue: "not a task", invokeTask: Task.CompletedTask);
            var context = CreateActivatorContext();
            await Assert.ThrowsAsync<AspectInvalidCastException>(() => activator.InvokeTask<int>(context));
        }

        [Fact]
        public async Task InvokeTask_WithFaultedTaskAndThrowAspect_ThrowsAspectInvocationException()
        {
            var faultedTask = Task.FromException(new InvalidOperationException("test error"));
            var activator = CreateActivator(invokeTask: faultedTask, throwAspectException: true);
            var context = CreateActivatorContext();
            var ex = await Assert.ThrowsAsync<AspectInvocationException>(() => activator.InvokeTask<int>(context));
            Assert.IsType<InvalidOperationException>(ex.InnerException);
        }

        [Fact]
        public async Task InvokeTask_WithFaultedTaskAndNoThrowAspect_ReThrowsOriginalException()
        {
            var faultedTask = Task.FromException(new InvalidOperationException("test error"));
            var activator = CreateActivator(invokeTask: faultedTask, throwAspectException: false);
            var context = CreateActivatorContext();
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => activator.InvokeTask<int>(context));
            Assert.Equal("test error", ex.Message);
        }

        [Fact]
        public async Task InvokeTask_WithRunningTask_AwaitsCompletion()
        {
            var runningTask = Task.Run(() => System.Threading.Thread.Sleep(50));
            var activator = CreateActivator(returnValue: Task.FromResult(99), invokeTask: runningTask);
            var context = CreateActivatorContext();
            var result = await activator.InvokeTask<int>(context);
            Assert.Equal(99, result);
        }

        [Fact]
        public async Task InvokeValueTask_WithNullReturnValue_ReturnsDefault()
        {
            var activator = CreateActivator(returnValue: null, invokeTask: Task.CompletedTask);
            var context = CreateActivatorContext();
            var result = await activator.InvokeValueTask<int>(context);
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task InvokeValueTask_WithValueTaskReturnValue_AwaitsAndReturns()
        {
            var innerTask = new ValueTask<int>(Task.FromResult(42));
            var activator = CreateActivator(returnValue: innerTask, invokeTask: Task.CompletedTask);
            var context = CreateActivatorContext();
            var result = await activator.InvokeValueTask<int>(context);
            Assert.Equal(42, result);
        }

        [Fact]
        public async Task InvokeValueTask_WithNonGenericValueTaskReturnValue_AwaitsAndReturnsDefault()
        {
            var innerTask = new ValueTask(Task.CompletedTask);
            var activator = CreateActivator(returnValue: innerTask, invokeTask: Task.CompletedTask);
            var context = CreateActivatorContext();
            var result = await activator.InvokeValueTask<int>(context);
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task InvokeValueTask_WithInvalidReturnValue_ThrowsAspectInvalidCastException()
        {
            var activator = CreateActivator(returnValue: "not a value task", invokeTask: Task.CompletedTask);
            var context = CreateActivatorContext();
            await Assert.ThrowsAsync<AspectInvalidCastException>(() => activator.InvokeValueTask<int>(context).AsTask());
        }

        [Fact]
        public async Task InvokeValueTask_WithFaultedTaskAndThrowAspect_ThrowsAspectInvocationException()
        {
            var faultedTask = Task.FromException(new InvalidOperationException("test error"));
            var activator = CreateActivator(invokeTask: faultedTask, throwAspectException: true);
            var context = CreateActivatorContext();
            var ex = await Assert.ThrowsAsync<AspectInvocationException>(() => activator.InvokeValueTask<int>(context).AsTask());
            Assert.IsType<InvalidOperationException>(ex.InnerException);
        }

        [Fact]
        public async Task InvokeValueTask_WithFaultedTaskAndNoThrowAspect_ReThrowsOriginalException()
        {
            var faultedTask = Task.FromException(new InvalidOperationException("test error"));
            var activator = CreateActivator(invokeTask: faultedTask, throwAspectException: false);
            var context = CreateActivatorContext();
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => activator.InvokeValueTask<int>(context).AsTask());
            Assert.Equal("test error", ex.Message);
        }

        [Fact]
        public void Invoke_ReleasesContext()
        {
            var released = false;
            var activator = CreateActivator(returnValue: 1, invokeTask: Task.CompletedTask, onRelease: ctx => released = true);
            var context = CreateActivatorContext();
            activator.Invoke<int>(context);
            Assert.True(released);
        }

        [Fact]
        public async Task InvokeTask_ReleasesContext()
        {
            var released = false;
            var activator = CreateActivator(returnValue: Task.FromResult(1), invokeTask: Task.CompletedTask, onRelease: ctx => released = true);
            var context = CreateActivatorContext();
            await activator.InvokeTask<int>(context);
            Assert.True(released);
        }

        [Fact]
        public async Task InvokeValueTask_ReleasesContext()
        {
            var released = false;
            var activator = CreateActivator(returnValue: new ValueTask<int>(Task.FromResult(1)), invokeTask: Task.CompletedTask, onRelease: ctx => released = true);
            var context = CreateActivatorContext();
            await activator.InvokeValueTask<int>(context);
            Assert.True(released);
        }

        public class TestService
        {
            public virtual void DoSomething() { }
        }

        private class FakeAspectContext : AspectContext
        {
            private object _returnValue;
            public FakeAspectContext(object returnValue) => _returnValue = returnValue;
            public override IDictionary<string, object> AdditionalData { get; } = new Dictionary<string, object>();
            public override object ReturnValue { get => _returnValue; set => _returnValue = value; }
            public override IServiceProvider ServiceProvider => null;
            public override MethodInfo ServiceMethod => null;
            public override object Implementation => null;
            public override MethodInfo ImplementationMethod => null;
            public override object[] Parameters => new object[0];
            public override MethodInfo ProxyMethod => null;
            public override MethodInfo PredicateMethod => null;
            public override object Proxy => null;
            public override Task Break() => Task.CompletedTask;
            public override Task Invoke(AspectDelegate next) => next(this);
            public override Task Complete() => Task.CompletedTask;
        }

        private class FakeAspectContextFactory : IAspectContextFactory
        {
            private readonly object _returnValue;
            private readonly Action<AspectContext> _onRelease;
            public FakeAspectContextFactory(object returnValue, Action<AspectContext> onRelease = null)
            {
                _returnValue = returnValue;
                _onRelease = onRelease;
            }
            public AspectContext CreateContext(AspectActivatorContext activatorContext) => new FakeAspectContext(_returnValue);
            public void ReleaseContext(AspectContext aspectContext) => _onRelease?.Invoke(aspectContext);
        }

        private class FakeAspectBuilderFactory : IAspectBuilderFactory
        {
            private readonly Task _invokeTask;
            public FakeAspectBuilderFactory(Task invokeTask) => _invokeTask = invokeTask;
            public IAspectBuilder Create(AspectContext context) => new FakeAspectBuilder(_invokeTask);
            public IAspectBuilder GetBuilder(MethodInfo serviceMethod, MethodInfo implementationMethod) => new FakeAspectBuilder(_invokeTask);
            public IAspectBuilder GetBuilder(MethodInfo serviceMethod, MethodInfo implementationMethod, MethodInfo predicateMethod) => new FakeAspectBuilder(_invokeTask);
        }

        private class FakeAspectBuilder : IAspectBuilder
        {
            private readonly Task _invokeTask;
            public FakeAspectBuilder(Task invokeTask) => _invokeTask = invokeTask;
            public IEnumerable<Func<AspectDelegate, AspectDelegate>> Delegates => new Func<AspectDelegate, AspectDelegate>[0];
            public AspectDelegate Build() => ctx => _invokeTask;
        }
    }
}
