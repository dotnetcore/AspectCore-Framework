using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.RedisProfiler
{
    [NonAspect]
    public interface IRedisProfilerCallbackHandler
    {
        Task HandleAsync(RedisProfilerCallbackHandlerContext profilerContext);
    }
}

//var callbacks = context.ServiceProvider.ResolveMany<IRedisProfilerCallback>();
//var callbackContext = new RedisProfilerCallbackContext();
//foreach (var callback in callbacks)
//    callback.Invoke(callbackContext);