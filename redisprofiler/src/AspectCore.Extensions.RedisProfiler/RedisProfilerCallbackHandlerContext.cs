using System;
using System.Collections.Generic;

namespace AspectCore.Extensions.RedisProfiler
{
    public sealed class RedisProfilerCallbackHandlerContext
    {
        public IEnumerable<RedisProfiledCommand> ProfiledCommands { get; }

        internal RedisProfilerCallbackHandlerContext(IEnumerable<RedisProfiledCommand> redisProfiledCommands)
        {
            ProfiledCommands = redisProfiledCommands ?? throw new ArgumentNullException(nameof(redisProfiledCommands));
        }
    }
}