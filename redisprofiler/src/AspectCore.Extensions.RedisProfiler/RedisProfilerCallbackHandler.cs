using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspectCore.Extensions.RedisProfiler
{
    public sealed class RedisProfilerCallbackHandler : IRedisProfilerCallbackHandler
    {
        private readonly IEnumerable<IRedisProfilerCallback> _redisProfilerCallbacks;

        public RedisProfilerCallbackHandler(IEnumerable<IRedisProfilerCallback> redisProfilerCallbacks)
        {
            _redisProfilerCallbacks = redisProfilerCallbacks ?? throw new ArgumentNullException(nameof(redisProfilerCallbacks));
        }
        public async Task HandleAsync(RedisProfilerCallbackHandlerContext profilerContext)
        {
            foreach (var callback in _redisProfilerCallbacks)
            {
                await callback.Invoke(new RedisProfilerCallbackContext(profilerContext));
            }
        }
    }
}