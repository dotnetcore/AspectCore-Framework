using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.Parameters;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class RuntimeAspectContextTests
    {
        public class TestService
        {
            public virtual int Add(int a, int b) => a + b;

            public virtual string GetName() => "test";

            public virtual Task<int> GetAsync() => Task.FromResult(42);

            public virtual void VoidMethod() { }

            public virtual string Concat(string prefix, int count) => prefix + count;
        }

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
            var defaultMethod = typeof(TestService).GetMethod(nameof(TestService.Add));
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

        private static MethodInfo GetMethod(string name) => typeof(TestService).GetMethod(name);

        // ---------- Constructor ----------

        [Fact]
        public void Constructor_SetsAllProperties()
        {
            var serviceProvider = new FakeServiceProvider();
            var target = new TestService();
            var proxy = new TestService();
            var method = GetMethod(nameof(TestService.Add));
            var parameters = new object[] { 10, 20 };

            var context = new RuntimeAspectContext(
                serviceProvider, method, method, method, method,
                target, proxy, parameters);

            Assert.Same(serviceProvider, context.ServiceProvider);
            Assert.Same(target, context.Implementation);
            Assert.Same(proxy, context.Proxy);
            Assert.Same(method, context.ServiceMethod);
            Assert.Same(method, context.ImplementationMethod);
            Assert.Same(method, context.ProxyMethod);
            Assert.Same(method, context.PredicateMethod);
            Assert.Same(parameters, context.Parameters);
        }

        // ---------- AdditionalData ----------

        [Fact]
        public void AdditionalData_FirstAccess_ReturnsNonNullDictionary()
        {
            var context = CreateContext();
            Assert.NotNull(context.AdditionalData);
            Assert.IsAssignableFrom<IDictionary<string, object>>(context.AdditionalData);
        }

        [Fact]
        public void AdditionalData_InitialCountIsZero()
        {
            var context = CreateContext();
            Assert.Empty(context.AdditionalData);
        }

        [Fact]
        public void AdditionalData_ReturnsSameInstance()
        {
            var context = CreateContext();
            var first = context.AdditionalData;
            var second = context.AdditionalData;
            Assert.Same(first, second);
        }

        [Fact]
        public void AdditionalData_CanAddAndRetrieveValues()
        {
            var context = CreateContext();
            context.AdditionalData["key1"] = "value1";
            context.AdditionalData["key2"] = 42;

            Assert.Equal("value1", context.AdditionalData["key1"]);
            Assert.Equal(42, context.AdditionalData["key2"]);
            Assert.Equal(2, context.AdditionalData.Count);
        }

        [Fact]
        public void AdditionalData_CanRemoveValues()
        {
            var context = CreateContext();
            context.AdditionalData["key1"] = "value1";
            Assert.True(context.AdditionalData.Remove("key1"));
            Assert.Empty(context.AdditionalData);
        }

        // ---------- Implementation ----------

        [Fact]
        public void Implementation_ReturnsTargetInstance()
        {
            var target = new TestService();
            var context = CreateContext(targetInstance: target);
            Assert.Same(target, context.Implementation);
        }

        [Fact]
        public void ImplementationMethod_ReturnsTargetMethod()
        {
            var method = GetMethod(nameof(TestService.GetName));
            var context = CreateContext(targetMethod: method);
            Assert.Same(method, context.ImplementationMethod);
        }

        // ---------- ReturnValue ----------

        [Fact]
        public void ReturnValue_DefaultIsNull()
        {
            var context = CreateContext();
            Assert.Null(context.ReturnValue);
        }

        [Fact]
        public void ReturnValue_CanSetAndGet()
        {
            var context = CreateContext();
            context.ReturnValue = "hello";
            Assert.Equal("hello", context.ReturnValue);

            context.ReturnValue = 123;
            Assert.Equal(123, context.ReturnValue);
        }

        // ---------- Parameters ----------

        [Fact]
        public void Parameters_ReturnsPassedParameters()
        {
            var parameters = new object[] { 5, 10 };
            var context = CreateContext(parameters: parameters);
            Assert.Same(parameters, context.Parameters);
            Assert.Equal(5, context.Parameters[0]);
            Assert.Equal(10, context.Parameters[1]);
        }

        [Fact]
        public void Parameters_CanModifyValues()
        {
            var context = CreateContext(parameters: new object[] { 5, 10 });
            context.Parameters[0] = 99;
            Assert.Equal(99, context.Parameters[0]);
        }

        // ---------- ServiceProvider ----------

        [Fact]
        public void ServiceProvider_ReturnsPassedProvider()
        {
            var serviceProvider = new FakeServiceProvider();
            var context = CreateContext(serviceProvider: serviceProvider);
            Assert.Same(serviceProvider, context.ServiceProvider);
        }

        [Fact]
        public void ServiceProvider_Null_ThrowsNotSupportedException()
        {
            var context = CreateContext(serviceProvider: null);
            Assert.Throws<NotSupportedException>(() => context.ServiceProvider);
        }

        // ---------- Break() ----------

        [Fact]
        public async Task Break_SetsReturnValueToDefault_WhenReturnValueIsNull()
        {
            // Add returns int; default for int is 0
            var context = CreateContext(serviceMethod: GetMethod(nameof(TestService.Add)));
            Assert.Null(context.ReturnValue);

            await context.Break();

            Assert.Equal(0, context.ReturnValue);
        }

        [Fact]
        public async Task Break_SetsReturnValueToNull_ForStringReturnType()
        {
            // GetName returns string; default for string is null
            var context = CreateContext(serviceMethod: GetMethod(nameof(TestService.GetName)));
            Assert.Null(context.ReturnValue);

            await context.Break();

            Assert.Null(context.ReturnValue);
        }

        [Fact]
        public async Task Break_DoesNotOverrideExistingReturnValue()
        {
            var context = CreateContext(serviceMethod: GetMethod(nameof(TestService.Add)));
            context.ReturnValue = 42;

            await context.Break();

            Assert.Equal(42, context.ReturnValue);
        }

        [Fact]
        public async Task Break_ReturnsCompletedTask()
        {
            var context = CreateContext(serviceMethod: GetMethod(nameof(TestService.Add)));
            var task = context.Break();
            Assert.True(task.IsCompleted);
            await task;
        }

        // ---------- Complete() ----------

        [Fact]
        public async Task Complete_InvokesImplementationMethod_AndSetsReturnValue()
        {
            var target = new TestService();
            var method = GetMethod(nameof(TestService.Add));
            var context = CreateContext(
                targetInstance: target,
                targetMethod: method,
                serviceMethod: method,
                parameters: new object[] { 3, 4 });

            await context.Complete();

            Assert.Equal(7, context.ReturnValue);
        }

        [Fact]
        public async Task Complete_StringMethod_SetsReturnValue()
        {
            var target = new TestService();
            var method = GetMethod(nameof(TestService.GetName));
            var context = CreateContext(
                targetInstance: target,
                targetMethod: method,
                serviceMethod: method,
                parameters: new object[0]);

            await context.Complete();

            Assert.Equal("test", context.ReturnValue);
        }

        [Fact]
        public async Task Complete_AsyncMethod_AwaitsAndSetsReturnValue()
        {
            var target = new TestService();
            var method = GetMethod(nameof(TestService.GetAsync));
            var context = CreateContext(
                targetInstance: target,
                targetMethod: method,
                serviceMethod: method,
                parameters: new object[0]);

            await context.Complete();

            // ReturnValue is the Task itself (unwrapping is separate);
            // the task must have been awaited and completed.
            var task = Assert.IsAssignableFrom<Task<int>>(context.ReturnValue);
            Assert.True(task.IsCompleted);
            Assert.Equal(42, await task);
        }

        [Fact]
        public async Task Complete_NullImplementation_CallsBreak()
        {
            var method = GetMethod(nameof(TestService.Add));
            // Construct directly with null target instance and null target method
            // so that Complete() takes the Break() path.
            var context = new RuntimeAspectContext(
                null, method, null, method, method,
                null, new TestService(), new object[] { 1, 2 });

            await context.Complete();

            // Break() should set the default value for int (0)
            Assert.Equal(0, context.ReturnValue);
        }

        // ---------- Invoke() ----------

        [Fact]
        public async Task Invoke_CallsNextDelegate_WithContext()
        {
            var context = CreateContext();
            AspectContext receivedContext = null;

            await context.Invoke(ctx =>
            {
                receivedContext = ctx;
                return Task.CompletedTask;
            });

            Assert.Same(context, receivedContext);
        }

        [Fact]
        public async Task Invoke_ReturnsTaskFromNextDelegate()
        {
            var context = CreateContext();
            var customTask = Task.FromResult("custom");

            var result = context.Invoke(ctx => customTask);

            Assert.Same(customTask, result);
            await result;
        }

        // ---------- GetParameters() extension ----------

        [Fact]
        public void GetParameters_ReturnsParameterCollectionWithCorrectCount()
        {
            var method = GetMethod(nameof(TestService.Add));
            var context = CreateContext(
                serviceMethod: method,
                parameters: new object[] { 1, 2 });

            var parameters = context.GetParameters();

            Assert.Equal(2, parameters.Count);
        }

        [Fact]
        public void GetParameters_ReturnsCorrectParameterNames()
        {
            var method = GetMethod(nameof(TestService.Add));
            var context = CreateContext(
                serviceMethod: method,
                parameters: new object[] { 1, 2 });

            var parameters = context.GetParameters();

            Assert.Equal("a", parameters[0].Name);
            Assert.Equal("b", parameters[1].Name);
        }

        [Fact]
        public void GetParameters_ReturnsCorrectParameterValues()
        {
            var method = GetMethod(nameof(TestService.Add));
            var context = CreateContext(
                serviceMethod: method,
                parameters: new object[] { 1, 2 });

            var parameters = context.GetParameters();

            Assert.Equal(1, parameters[0].Value);
            Assert.Equal(2, parameters[1].Value);
        }

        [Fact]
        public void GetParameters_ReturnsCorrectParameterTypes()
        {
            var method = GetMethod(nameof(TestService.Concat));
            var context = CreateContext(
                serviceMethod: method,
                parameters: new object[] { "prefix", 5 });

            var parameters = context.GetParameters();

            Assert.Equal(typeof(string), parameters[0].Type);
            Assert.Equal(typeof(int), parameters[1].Type);
        }

        [Fact]
        public void GetParameters_CanLookupByName()
        {
            var method = GetMethod(nameof(TestService.Add));
            var context = CreateContext(
                serviceMethod: method,
                parameters: new object[] { 1, 2 });

            var parameters = context.GetParameters();

            Assert.Equal(1, parameters["a"].Value);
            Assert.Equal(2, parameters["b"].Value);
        }

        [Fact]
        public void GetParameters_NoParameters_ReturnsEmptyCollection()
        {
            var method = GetMethod(nameof(TestService.GetName));
            var context = CreateContext(
                serviceMethod: method,
                parameters: new object[0]);

            var parameters = context.GetParameters();

            Assert.Empty(parameters);
        }

        [Fact]
        public void GetParameters_GetValues_ReturnsAllValues()
        {
            var method = GetMethod(nameof(TestService.Add));
            var context = CreateContext(
                serviceMethod: method,
                parameters: new object[] { 1, 2 });

            var values = context.GetParameters().GetValues();

            Assert.Equal(new object[] { 1, 2 }, values);
        }

        // ---------- IsAsync() extension ----------

        [Fact]
        public void IsAsync_SyncMethod_ReturnsFalse()
        {
            var method = GetMethod(nameof(TestService.Add));
            var context = CreateContext(serviceMethod: method);
            Assert.False(context.IsAsync());
        }

        [Fact]
        public void IsAsync_TaskMethod_ReturnsTrue()
        {
            var method = GetMethod(nameof(TestService.GetAsync));
            var context = CreateContext(serviceMethod: method);
            Assert.True(context.IsAsync());
        }

        // ---------- AwaitIfAsync() extension ----------

        [Fact]
        public async Task AwaitIfAsync_NullReturnValue_DoesNotThrow()
        {
            var context = CreateContext();
            context.ReturnValue = null;
            await context.AwaitIfAsync();
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
        public async Task AwaitIfAsync_NonAsyncReturnValue_DoesNotThrow()
        {
            var context = CreateContext();
            context.ReturnValue = "not a task";
            await context.AwaitIfAsync();
            Assert.Equal("not a task", context.ReturnValue);
        }

        // ---------- UnwrapAsyncReturnValue() extension ----------

        [Fact]
        public async Task UnwrapAsyncReturnValue_TaskResult_ReturnsResult()
        {
            var method = GetMethod(nameof(TestService.GetAsync));
            var context = CreateContext(serviceMethod: method);
            context.ReturnValue = Task.FromResult(99);

            var result = await context.UnwrapAsyncReturnValue();

            Assert.Equal(99, result);
        }

        [Fact]
        public async Task UnwrapAsyncReturnValue_Typed_TaskResult_ReturnsResult()
        {
            var method = GetMethod(nameof(TestService.GetAsync));
            var context = CreateContext(serviceMethod: method);
            context.ReturnValue = Task.FromResult(99);

            var result = await context.UnwrapAsyncReturnValue<int>();

            Assert.Equal(99, result);
        }

        [Fact]
        public async Task UnwrapAsyncReturnValue_NonAsync_Throws()
        {
            var method = GetMethod(nameof(TestService.Add));
            var context = CreateContext(serviceMethod: method);
            context.ReturnValue = 42;

            await Assert.ThrowsAsync<AspectInvalidOperationException>(() => context.UnwrapAsyncReturnValue());
        }

        // ---------- Dispose() ----------

        [Fact]
        public void Dispose_ClearsAdditionalData()
        {
            var context = CreateContext();
            context.AdditionalData["key1"] = "value1";
            context.AdditionalData["key2"] = 42;

            context.Dispose();

            Assert.Empty(context.AdditionalData);
        }

        [Fact]
        public void Dispose_DisposesDisposableValuesInAdditionalData()
        {
            var context = CreateContext();
            var disposable = new FakeDisposable();
            context.AdditionalData["disposable"] = disposable;

            context.Dispose();

            Assert.True(disposable.Disposed);
        }

        [Fact]
        public void Dispose_DoesNotDisposeNonDisposableValues()
        {
            var context = CreateContext();
            context.AdditionalData["key1"] = "value1";
            context.AdditionalData["key2"] = 42;

            // Should not throw
            context.Dispose();
        }

        [Fact]
        public void Dispose_CanBeCalledMultipleTimes()
        {
            var context = CreateContext();
            context.AdditionalData["key1"] = "value1";

            context.Dispose();
            context.Dispose();

            Assert.Empty(context.AdditionalData);
        }

        [Fact]
        public void Dispose_WithoutAdditionalData_DoesNotThrow()
        {
            var context = CreateContext();
            // Don't access AdditionalData so _data remains null
            context.Dispose();
        }

        // ---------- Helpers ----------

        private class FakeServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType) => null;
        }

        private class FakeDisposable : IDisposable
        {
            public bool Disposed { get; private set; }

            public void Dispose()
            {
                Disposed = true;
            }
        }
    }
}
