using System.Threading.Tasks;
using AspectCore.Utils;
using Xunit;

namespace AspectCore.Core.Tests.Utils
{
    public class TaskUtilsTests
    {
        [Fact]
        public void CompletedTask_IsCompleted()
        {
            Assert.NotNull(TaskUtils.CompletedTask);
            Assert.Equal(TaskStatus.RanToCompletion, TaskUtils.CompletedTask.Status);
        }

        [Fact]
        public void CompletedTask_Generic_IsCompleted()
        {
            Assert.NotNull(TaskUtils<int>.CompletedTask);
            Assert.Equal(TaskStatus.RanToCompletion, TaskUtils<int>.CompletedTask.Status);
        }

        [Fact]
        public async Task CompletedTask_Generic_ReturnsDefaultValue()
        {
            Assert.Equal(0, await TaskUtils<int>.CompletedTask);
            Assert.Null(await TaskUtils<string>.CompletedTask);
            Assert.Null(await TaskUtils<object>.CompletedTask);
        }
    }
}
