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