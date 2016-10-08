using AspectCore.Lite.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AspectCore.Lite.Test.Extensions
{
    public class TaskExtensionsTest
    {
        [Theory]
        [InlineData(0)]
        [InlineData("test")]
        [InlineData(null)]
        public void Task_FromResult_Execute_Test(object result)
        {
            var task = Task.FromResult(result);
            Assert.Equal(task.WaitWithAsync() , result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100)]
        [InlineData(1000)]
        [InlineData(5000)]
        public void Task_Delay_Execute_Test(int millisecondsDelay)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            var task = Task.Delay(millisecondsDelay);
            task.WaitWithAsync();
            Assert.True(stopwatch.ElapsedMilliseconds >= millisecondsDelay);
        }

        [Theory]
        [InlineData(0)]
        [InlineData("test")]
        [InlineData(null)]
        public void Task_Run_Execute_Test(object result)
        {
            var task = Task.Run(() => result);
            Assert.Equal(task.WaitWithAsync() , result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData("test")]
        [InlineData(null)]
        public void Task_ContinueWith_Execute_Test(object result)
        {
            var task = Task.Run(() => result).ContinueWith(t => t.Result);
            Assert.Equal(task.WaitWithAsync() , result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData("test")]
        [InlineData(null)]
        public void Task_YieldAsync_Execute_Test(object result)
        {
            var task = Yield_Test_Async(result);
            Assert.Equal(task.WaitWithAsync() , result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData("test")]
        [InlineData(null)]
        public void Task_RunAsync_Execute_Test(object result)
        {
            var task = Run_Test_Async(result);
            Assert.Equal(task.WaitWithAsync() , result);
        }

        private async Task<object> Yield_Test_Async(object data)
        {
            await Task.Yield();
            return data;
        }

        private async Task<object> Run_Test_Async(object data)
        {
            var result = await Task.Run(() => data);
            return result;
        }
    }
}
