using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Autofac.Test
{
    [ExecutionTimerInterceptor]
    public class TaskRepository : ITaskRepository
    {
        public virtual int Id { get; set; } = 101;

        public Logger Logger { get; set; }

        public TaskRepository(Logger logger)
        {
            Logger = logger;
        }
    }
}
