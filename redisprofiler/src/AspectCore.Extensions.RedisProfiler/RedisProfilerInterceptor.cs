using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.Injector;
using StackExchange.Redis;

#pragma warning disable 4014

namespace AspectCore.Extensions.RedisProfiler
{
    public sealed class RedisProfilerInterceptor : AbstractInterceptor
    {
        public async override Task Invoke(AspectContext context, AspectDelegate next)
        {
            var connectionMultiplexer = context.ServiceProvider.ResolveRequired<IConnectionMultiplexer>();
            var redisProfilerCallbackHandler = context.ServiceProvider.ResolveRequired<IRedisProfilerCallbackHandler>();
            var profilerContext = new object();
            AspectRedisDatabaseProfilerContext.Context = profilerContext;
            connectionMultiplexer.BeginProfiling(profilerContext);
            await context.Invoke(next);
            var profiledCommands = connectionMultiplexer.FinishProfiling(profilerContext);
            redisProfilerCallbackHandler.HandleAsync(null);
            AspectRedisDatabaseProfilerContext.Context = null;
        }
    }
}