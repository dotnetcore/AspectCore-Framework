using System;
using System.Collections.Generic;

namespace AspectCore.Extensions.RedisProfiler
{
    public sealed class RedisProfilerCallbackContext
    {
        public IEnumerable<RedisProfiledCommand> ProfiledCommands { get; }

        internal RedisProfilerCallbackContext(RedisProfilerCallbackHandlerContext redisProfilerCallbackHandlerContext)
        {
            ProfiledCommands = redisProfilerCallbackHandlerContext?.ProfiledCommands ?? throw new ArgumentNullException("profiledCommands");
        }
    }
}
