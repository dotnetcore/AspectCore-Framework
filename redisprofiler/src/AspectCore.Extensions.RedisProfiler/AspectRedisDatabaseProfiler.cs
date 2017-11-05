using AspectCore.DynamicProxy;
using StackExchange.Redis;


namespace AspectCore.Extensions.RedisProfiler
{
    [NonAspect]
    public sealed class AspectRedisDatabaseProfiler : IProfiler
    {
        public object GetContext()
        {
            return AspectRedisDatabaseProfilerContext.Context;
        }
    }
}