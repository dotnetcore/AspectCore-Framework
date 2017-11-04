using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AspectCore.Extensions.RedisProfiler
{
    public interface IRedisProfilerCallback
    {
        Task ExecuteAsync(RedisProfilerCallbackContext callbackContext);
    }
}