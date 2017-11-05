using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.RedisProfiler
{
    [NonAspect]
    public interface IRedisProfilerCallback
    {
        Task Invoke(RedisProfilerCallbackContext callbackContext);
    }
}