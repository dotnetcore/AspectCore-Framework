using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.Injector;
using StackExchange.Redis;

namespace AspectCore.Extensions.RedisProfiler
{
    public sealed class SubscriberProxyInterceptor : AbstractInterceptor
    {
        public async override Task Invoke(AspectContext context, AspectDelegate next)
        {
            await context.Invoke(next);
            if (context.Implementation is IConnectionMultiplexer connectionMultiplexer)
            {
                var subscriber = (ISubscriber)context.ReturnValue;
                if (!subscriber.IsProxy())
                {
                    var proxyGenerator = context.ServiceProvider.Resolve<IProxyGenerator>();
                    context.ReturnValue = proxyGenerator.CreateInterfaceProxy<ISubscriber>(subscriber);
                }
            }
        }
    }
}