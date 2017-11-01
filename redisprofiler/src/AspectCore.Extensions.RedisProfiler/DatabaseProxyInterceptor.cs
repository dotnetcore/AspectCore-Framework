using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using StackExchange.Redis;

namespace AspectCore.Extensions.RedisProfiler
{
    public class DatabaseProxyInterceptor : AbstractInterceptor
    {
        public async override Task Invoke(AspectContext context, AspectDelegate next)
        {
            await context.Invoke(next);
            if (context.Implementation is IConnectionMultiplexer connectionMultiplexer)
            {
                var database = (IDatabase)context.ReturnValue;
                if (!database.IsProxy())
                {
                    var proxyGenerator = (IProxyGenerator)context.ServiceProvider.GetService(typeof(IProxyGenerator));
                    context.ReturnValue = proxyGenerator.CreateInterfaceProxy<IDatabase>(database);
                }
            }
        }
    }
}