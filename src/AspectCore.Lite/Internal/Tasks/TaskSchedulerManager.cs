using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Internal.Tasks
{
    internal static class TaskSchedulerManager<TTaskScheduler>
    {
        private readonly static Lazy<TTaskScheduler> lazy = new Lazy<TTaskScheduler>();

        public static TTaskScheduler Default => lazy.Value;
    }
}
