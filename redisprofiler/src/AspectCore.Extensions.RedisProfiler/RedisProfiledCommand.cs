using System;
using System.Net;

namespace AspectCore.Extensions.RedisProfiler
{
    public struct RedisProfiledCommand
    {
        public EndPoint Server { get; private set; }

        public int Db { get; private set; }

        public string Command { get; private set; }

        public DateTime CommandCreated { get; private set; }

        public TimeSpan Elapsed { get; private set; }

        public override string ToString()
        {
            return $"server-{Server}  db-{Db}  command-{Command}  elapsed-{Elapsed}";
        }

        internal static RedisProfiledCommand Create(string command, EndPoint server, int db, DateTime commandCreated, TimeSpan elapsed)
        {
            var redisProfiledCommand = new RedisProfiledCommand();
            redisProfiledCommand.Command = command;
            redisProfiledCommand.Server = server;
            redisProfiledCommand.Db = db;
            redisProfiledCommand.CommandCreated = commandCreated;
            redisProfiledCommand.Elapsed = elapsed;
            return redisProfiledCommand;
        }
    }
}