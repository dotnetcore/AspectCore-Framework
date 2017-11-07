using System;
using AspectCore.Configuration;
using AspectCore.Injector;
using StackExchange.Redis;

namespace AspectCore.Extensions.RedisProfiler
{
    public static class ServiceContainerExtensions
    {
        public static IServiceContainer AddRedisProfiler(this IServiceContainer services, IConnectionMultiplexer connectionMultiplexer)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (connectionMultiplexer == null)
            {
                throw new ArgumentNullException(nameof(connectionMultiplexer));
            }
            connectionMultiplexer.RegisterProfiler(new AspectRedisDatabaseProfiler());
            services.AddInstance<IConnectionMultiplexer>(connectionMultiplexer);
            services.AddType<IRedisProfilerCallbackHandler, RedisProfilerCallbackHandler>();
            services.Configure(config =>
            {
                config.Interceptors.AddTyped<ServerProxyInterceptor>(
                    Predicates.ForService(typeof(IRedis).FullName),
                    Predicates.ForService(typeof(IRedisAsync).FullName),
                    Predicates.ForService(typeof(IDatabase).FullName),
                    Predicates.ForService(typeof(IDatabaseAsync).FullName),
                    Predicates.ForService(typeof(IServer).FullName),
                    Predicates.ForService(typeof(ISubscriber).FullName));
                config.Interceptors.AddTyped<DatabaseProxyInterceptor>(
                    Predicates.ForMethod(typeof(IConnectionMultiplexer).FullName, nameof(IConnectionMultiplexer.GetDatabase)));
                config.Interceptors.AddTyped<ServerProxyInterceptor>(
                    Predicates.ForMethod(typeof(IConnectionMultiplexer).FullName, nameof(IConnectionMultiplexer.GetServer)));
                config.Interceptors.AddTyped<SubscriberProxyInterceptor>(
                    Predicates.ForMethod(typeof(IConnectionMultiplexer).FullName, nameof(IConnectionMultiplexer.GetSubscriber)));
            });
            return services;
        }
    }
}