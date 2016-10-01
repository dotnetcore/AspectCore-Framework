using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using AspectCore.Lite.Extensions;

namespace AspectCore.Lite.Test.Abstractions.Tasks
{
    public class TaskAwaitableTest
    {
        [Fact]
        public async Task AsAwaitable_Test()
        {
            var task = Task.Run(() => "test");
            var result = task.AsAwaitable().AwaitResult();
            Assert.Equal(await task , result);
        }
    }
}
