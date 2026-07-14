using System;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class AspectContextRuntimeExtensionsTests
    {
        public class TestService
        {
            public virtual int Add(int a, int b) => a + b;

            public virtual string GetName() => "test";

            public virtual Task<int> GetAsync() => Task.FromResult(42);

            public virtual Task GetTaskAsync() => Task.CompletedTask;

            public virtual ValueTask<int> GetValueTaskAsync() => new ValueTask<int>(42);

            public virtual ValueTask GetValueTask() => new ValueTask(Task.CompletedTask);

            public virtual Task<string> GetStringAsync() => Task.FromResult("hello");

            public virtual void VoidMethod() { }

            [AsyncAspect]
            public virtual object AsyncAspectMethod() => 42;

            public virtual Task<Task<int>> NestedTask() => Task.FromResult(Task.FromResult(99));
        }

        private static MethodInfo GetMethod(string name) => typeof(TestService).GetMethod(name);

        private static RuntimeAspectContext CreateContext(
            IServiceProvider serviceProvider = null,
            object targetInstance = null,
            object[] parameters = null,
            MethodInfo serviceMethod = null,
            MethodInfo targetMethod = null,
            MethodInfo proxyMethod = null,
            MethodInfo predicateMethod = null,
            object proxyInstance = null)
        {
            var defaultMethod = GetMethod(nameof(TestService.Add));
            return new RuntimeAspectContext(
                serviceProvider,
                serviceMethod ?? defaultMethod,
                targetMethod ?? defaultMethod,
                proxyMethod ?? defaultMethod,
                predicateMethod ?? defaultMethod,
                targetInstance ?? new TestService(),
                proxyInstance ?? new TestService(),
                parameters ?? new object[] { 1, 2 });
        }

        private class FakeServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType) => null;
        }

        // ---------- AwaitIfAsync(AspectContext) ----------

        [Fact]
        public async Task AwaitIfAsync_NullReturnValue_DoesNotThrow()
        {
            var context = CreateContext();
            context.ReturnValue = null;
            await context.AwaitIfAsync();
            Assert.Null(context.ReturnValue);
        }

        [Fact]
        public async Task AwaitIfAsync_TaskReturnValue_AwaitsTask()
        {
            var context = CreateContext();
            context.ReturnValue = Task.Delay(10);
            await context.AwaitIfAsync();
            Assert.True(((Task)context.ReturnValue).IsCompleted);
        }

        [Fact]
        public async Task AwaitIfAsync_ValueTaskReturnValue_AwaitsValueTask()
        {
            var context = CreateContext();
            context.ReturnValue = new ValueTask(Task.Delay(10));
            await context.AwaitIfAsync();
        }

        [Fact]
        public async Task AwaitIfAsync_ValueTaskWithResult_AwaitsValueTask()
        {
            var context = CreateContext();
            context.ReturnValue = new ValueTask<int>(42);
            await context.AwaitIfAsync();
        }

        [Fact]
        public async Task AwaitIfAsync_NonAsyncReturnValue_DoesNotThrow()
        {
            var context = CreateContext();
            context.ReturnValue = "not a task";
            await context.AwaitIfAsync();
            Assert.Equal("not a task", context.ReturnValue);
        }

        // ---------- AwaitIfAsync(AspectContext, object) ----------

        [Fact]
        public async Task AwaitIfAsync_WithReturnValue_Null_DoesNotThrow()
        {
            var context = CreateContext();
            await context.AwaitIfAsync(null);
        }

        [Fact]
        public async Task AwaitIfAsync_WithReturnValue_Task_AwaitsTask()
        {
            var context = CreateContext();
            var task = Task.Delay(10);
            await context.AwaitIfAsync(task);
            Assert.True(task.IsCompleted);
        }

        [Fact]
        public async Task AwaitIfAsync_WithReturnValue_ValueTask_AwaitsValueTask()
        {
            var context = CreateContext();
            var valueTask = new ValueTask(Task.Delay(10));
            await context.AwaitIfAsync(valueTask);
        }

        [Fact]
        public async Task AwaitIfAsync_WithReturnValue_ValueTaskWithResult_AwaitsValueTask()
        {
            var context = CreateContext();
            var valueTask = new ValueTask<int>(42);
            await context.AwaitIfAsync(valueTask);
        }

        [Fact]
        public async Task AwaitIfAsync_WithReturnValue_NonAsync_DoesNotThrow()
        {
            var context = CreateContext();
            await context.AwaitIfAsync("not a task");
        }

        // ---------- IsAsync ----------

        [Fact]
        public void IsAsync_NullContext_ThrowsArgumentNullException()
        {
            AspectContext context = null;
            Assert.Throws<ArgumentNullException>(() => context.IsAsync());
        }

        [Fact]
        public void IsAsync_SyncMethod_ReturnsFalse()
        {
            var context = CreateContext(serviceMethod: GetMethod(nameof(TestService.Add)));
            Assert.False(context.IsAsync());
        }

        [Fact]
        public void IsAsync_TaskMethod_ReturnsTrue()
        {
            var context = CreateContext(serviceMethod: GetMethod(nameof(TestService.GetAsync)));
            Assert.True(context.IsAsync());
        }

        [Fact]
        public void IsAsync_VoidTaskMethod_ReturnsTrue()
        {
            var context = CreateContext(serviceMethod: GetMethod(nameof(TestService.GetTaskAsync)));
            Assert.True(context.IsAsync());
        }

        [Fact]
        public void IsAsync_ValueTaskMethod_ReturnsTrue()
        {
            var context = CreateContext(serviceMethod: GetMethod(nameof(TestService.GetValueTask)));
            Assert.True(context.IsAsync());
        }

        [Fact]
        public void IsAsync_ValueTaskWithResultMethod_ReturnsTrue()
        {
            var context = CreateContext(serviceMethod: GetMethod(nameof(TestService.GetValueTaskAsync)));
            Assert.True(context.IsAsync());
        }

        [Fact]
        public void IsAsync_AsyncAspectAttributeWithObjectReturn_ReturnsTrue()
        {
            var context = CreateContext(serviceMethod: GetMethod(nameof(TestService.AsyncAspectMethod)));
            Assert.True(context.IsAsync());
        }

        [Fact]
        public void IsAsync_SyncMethodWithTaskReturnValue_ReturnsTrue()
        {
            // Service method is sync (Add returns int), but ReturnValue is a Task.
            var context = CreateContext(serviceMethod: GetMethod(nameof(TestService.Add)));
            context.ReturnValue = Task.FromResult(42);
            Assert.True(context.IsAsync());
        }

        [Fact]
        public void IsAsync_SyncMethodWithNullReturnValue_ReturnsFalse()
        {
            var context = CreateContext(serviceMethod: GetMethod(nameof(TestService.Add)));
            context.ReturnValue = null;
            Assert.False(context.IsAsync());
        }

        // ---------- UnwrapAsyncReturnValue ----------

        [Fact]
        public async Task UnwrapAsyncReturnValue_NullContext_ThrowsArgumentNullException()
        {
            AspectContext context = null;
            await Assert.ThrowsAsync<ArgumentNullException>(() => context.UnwrapAsyncReturnValue());
        }

        [Fact]
        public async Task UnwrapAsyncReturnValue_NonAsyncMethod_ThrowsAspectInvalidOperationException()
        {
            var context = CreateContext(serviceMethod: GetMethod(nameof(TestService.Add)));
            context.ReturnValue = 42;

            await Assert.ThrowsAsync<AspectInvalidOperationException>(() => context.UnwrapAsyncReturnValue());
        }

        [Fact]
        public async Task UnwrapAsyncReturnValue_TaskWithResult_ReturnsResult()
        {
            var context = CreateContext(serviceMethod: GetMethod(nameof(TestService.GetAsync)));
            context.ReturnValue = Task.FromResult(99);

            var result = await context.UnwrapAsyncReturnValue();

            Assert.Equal(99, result);
        }

        [Fact]
        public async Task UnwrapAsyncReturnValue_TaskWithStringResult_ReturnsResult()
        {
            var context = CreateContext(serviceMethod: GetMethod(nameof(TestService.GetStringAsync)));
            context.ReturnValue = Task.FromResult("hello world");

            var result = await context.UnwrapAsyncReturnValue();

            Assert.Equal("hello world", result);
        }

        [Fact]
        public async Task UnwrapAsyncReturnValue_VoidTask_ReturnsNull()
        {
            var context = CreateContext(serviceMethod: GetMethod(nameof(TestService.GetTaskAsync)));
            context.ReturnValue = Task.CompletedTask;

            var result = await context.UnwrapAsyncReturnValue();

            Assert.Null(result);
        }

        [Fact]
        public async Task UnwrapAsyncReturnValue_ValueTaskWithResult_ReturnsResult()
        {
            var context = CreateContext(serviceMethod: GetMethod(nameof(TestService.GetValueTaskAsync)));
            context.ReturnValue = new ValueTask<int>(77);

            var result = await context.UnwrapAsyncReturnValue();

            Assert.Equal(77, result);
        }

        [Fact]
        public async Task UnwrapAsyncReturnValue_VoidValueTask_ReturnsNull()
        {
            var context = CreateContext(serviceMethod: GetMethod(nameof(TestService.GetValueTask)));
            context.ReturnValue = new ValueTask(Task.CompletedTask);

            var result = await context.UnwrapAsyncReturnValue();

            Assert.Null(result);
        }

        [Fact]
        public async Task UnwrapAsyncReturnValue_NullReturnValue_ReturnsNull()
        {
            var context = CreateContext(serviceMethod: GetMethod(nameof(TestService.GetAsync)));
            context.ReturnValue = null;

            var result = await context.UnwrapAsyncReturnValue();

            Assert.Null(result);
        }

        [Fact]
        public async Task UnwrapAsyncReturnValue_NestedTask_UnwrapsInnerTaskResult()
        {
            var context = CreateContext(serviceMethod: GetMethod(nameof(TestService.NestedTask)));
            context.ReturnValue = Task.FromResult(Task.FromResult(99));

            var result = await context.UnwrapAsyncReturnValue();

            Assert.Equal(99, result);
        }

        // ---------- UnwrapAsyncReturnValue<T> ----------

        [Fact]
        public async Task UnwrapAsyncReturnValue_Typed_TaskWithResult_ReturnsResult()
        {
            var context = CreateContext(serviceMethod: GetMethod(nameof(TestService.GetAsync)));
            context.ReturnValue = Task.FromResult(99);

            var result = await context.UnwrapAsyncReturnValue<int>();

            Assert.Equal(99, result);
        }

        [Fact]
        public async Task UnwrapAsyncReturnValue_Typed_TaskWithStringResult_ReturnsResult()
        {
            var context = CreateContext(serviceMethod: GetMethod(nameof(TestService.GetStringAsync)));
            context.ReturnValue = Task.FromResult("hello");

            var result = await context.UnwrapAsyncReturnValue<string>();

            Assert.Equal("hello", result);
        }

        [Fact]
        public async Task UnwrapAsyncReturnValue_Typed_ValueTaskWithResult_ReturnsResult()
        {
            var context = CreateContext(serviceMethod: GetMethod(nameof(TestService.GetValueTaskAsync)));
            context.ReturnValue = new ValueTask<int>(77);

            var result = await context.UnwrapAsyncReturnValue<int>();

            Assert.Equal(77, result);
        }

        // ---------- CreateFuncToGetTaskResult (internal, for testing) ----------

        [Fact]
        public void CreateFuncToGetTaskResult_TaskInt_ReturnsCorrectResult()
        {
            var task = Task.FromResult(42);
            var func = AspectContextRuntimeExtensions.CreateFuncToGetTaskResult(typeof(Task<int>));

            var result = func(task);

            Assert.Equal(42, result);
        }

        [Fact]
        public void CreateFuncToGetTaskResult_TaskString_ReturnsCorrectResult()
        {
            var task = Task.FromResult("hello");
            var func = AspectContextRuntimeExtensions.CreateFuncToGetTaskResult(typeof(Task<string>));

            var result = func(task);

            Assert.Equal("hello", result);
        }

        [Fact]
        public void CreateFuncToGetTaskResult_TaskObject_ReturnsCorrectResult()
        {
            var expected = new object();
            var task = Task.FromResult(expected);
            var func = AspectContextRuntimeExtensions.CreateFuncToGetTaskResult(typeof(Task<object>));

            var result = func(task);

            Assert.Same(expected, result);
        }

        [Fact]
        public void CreateFuncToGetTaskResult_DifferentTaskTypes_ReturnsTypeSpecificFuncs()
        {
            var intTask = Task.FromResult(42);
            var stringTask = Task.FromResult("test");

            var intFunc = AspectContextRuntimeExtensions.CreateFuncToGetTaskResult(typeof(Task<int>));
            var stringFunc = AspectContextRuntimeExtensions.CreateFuncToGetTaskResult(typeof(Task<string>));

            Assert.Equal(42, intFunc(intTask));
            Assert.Equal("test", stringFunc(stringTask));
        }
    }
}
