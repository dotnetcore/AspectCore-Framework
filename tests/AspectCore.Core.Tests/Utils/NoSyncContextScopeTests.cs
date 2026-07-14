using System.Threading;
using System.Threading.Tasks;
using AspectCore.Utils;
using Xunit;

namespace AspectCore.Core.Tests.Utils
{
    public class NoSyncContextScopeTests
    {
        [Fact]
        public void Run_WithCompletedTask_DoesNotThrow()
        {
            var task = Task.CompletedTask;
            NoSyncContextScope.Run(task);
        }

        [Fact]
        public void Run_WithFaultedTask_ThrowsInnerException()
        {
            var task = Task.FromException(new System.InvalidOperationException("test error"));
            var ex = Assert.Throws<System.InvalidOperationException>(() => NoSyncContextScope.Run(task));
            Assert.Equal("test error", ex.Message);
        }

        [Fact]
        public void Run_WithRunningTask_WaitsForCompletion()
        {
            var task = Task.Run(() => Thread.Sleep(50));
            NoSyncContextScope.Run(task);
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
        }

        [Fact]
        public void Run_Generic_WithCompletedTask_ReturnsResult()
        {
            var task = Task.FromResult(42);
            var result = NoSyncContextScope.Run(task);
            Assert.Equal(42, result);
        }

        [Fact]
        public void Run_Generic_WithFaultedTask_ThrowsInnerException()
        {
            var task = Task.FromException<int>(new System.InvalidOperationException("test error"));
            var ex = Assert.Throws<System.InvalidOperationException>(() => NoSyncContextScope.Run(task));
            Assert.Equal("test error", ex.Message);
        }

        [Fact]
        public void Run_Generic_WithRunningTask_WaitsAndReturnsResult()
        {
            var task = Task.Run(() =>
            {
                Thread.Sleep(50);
                return "hello";
            });
            var result = NoSyncContextScope.Run(task);
            Assert.Equal("hello", result);
        }

        [Fact]
        public void Run_RestoresSynchronizationContextAfterExecution()
        {
            var originalContext = SynchronizationContext.Current;
            var task = Task.Delay(10);
            NoSyncContextScope.Run(task);
            Assert.Equal(originalContext, SynchronizationContext.Current);
        }

        [Fact]
        public void Run_Generic_RestoresSynchronizationContextAfterExecution()
        {
            var originalContext = SynchronizationContext.Current;
            var task = Task.FromResult(1);
            NoSyncContextScope.Run(task);
            Assert.Equal(originalContext, SynchronizationContext.Current);
        }
    }
}
