using System;
using AspectCore.Injector;
using AspectCore.Configuration;
using StackExchange.Redis;
using AspectCore.Extensions.RedisProfiler;
using System.Collections.Generic;
using System.Threading.Tasks;
using InfluxDB.LineProtocol.Client;
using InfluxDB.LineProtocol.Payload;

namespace RedisProfiler.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceContainer();

            var connection = ConnectionMultiplexer.Connect("localhost:6379,allowAdmin=true");
            services.AddInstance<IConnectionMultiplexer>(connection);

            #region AddRedisProfiler

            services.AddRedisProfiler(connection);

            services.AddType<IRedisProfilerCallback, ConsoleProfilerCallback>(Lifetime.Singleton);

            #endregion

            var resolver = services.Build();

            var multiplexer = resolver.ResolveRequired<IConnectionMultiplexer>();

            var db = multiplexer.GetDatabase(0);

            var values = new List<KeyValuePair<RedisKey, RedisValue>>();
            values.Add(new KeyValuePair<RedisKey, RedisValue>("test1", "val"));
            values.Add(new KeyValuePair<RedisKey, RedisValue>("test2", "val"));
            values.Add(new KeyValuePair<RedisKey, RedisValue>("test3", "val"));

            db.StringSetAsync(values.ToArray());

            db.StringGet("test1");

            db.HashGetAll("hashtest");

            var subscriber = multiplexer.GetSubscriber();

            subscriber.Subscribe("test", (rc, rv) =>
            {
                Console.WriteLine("{0}-{1}", rc, rv);
            });

            subscriber.Publish("test", "Publish test");

            subscriber.Unsubscribe("test");

            Console.ReadKey();
        }
    }

    public class ConsoleProfilerCallback : IRedisProfilerCallback
    {
        private readonly LineProtocolClient _lineProtocolClient;
        public ConsoleProfilerCallback()
        {
            _lineProtocolClient = new LineProtocolClient(new Uri("http://localhost.:8186"), "redis_profiler");
        }

        public async Task Invoke(RedisProfilerCallbackContext callbackContext)
        {
            var payload = new LineProtocolPayload();
            foreach (var command in callbackContext.ProfiledCommands)
            {
                Console.WriteLine(command);
                var fields = new Dictionary<string, object>();
                fields.Add("elapsed", command.Elapsed.Milliseconds);
                var tags = new Dictionary<string, string>();
                tags.Add("application", "RedisProfiler.Sample");
                tags.Add("command", command.Command);
                tags.Add("redis_server", command.Server.ToString());
                payload.Add(new LineProtocolPoint("redis_command", fields, tags));
            }
            await _lineProtocolClient.WriteAsync(payload);
        }
    }
}
