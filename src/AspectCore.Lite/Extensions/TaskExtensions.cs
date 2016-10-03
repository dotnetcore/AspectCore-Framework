using AspectCore.Lite.Internal.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspectCore.Lite.Extensions
{
    public static class TaskExtensions
    {
        public static void Execute(this Task task , TaskExecutingOptions options = TaskExecutingOptions.CurrentThread)
        {
            if(task==null)
            {
                throw new ArgumentNullException(nameof(task));
            }
            var executingTask = Task.Factory.StartNew(() => task , CancellationToken.None , TaskCreationOptions.None , GetTaskScheduler(options)).Unwrap();
            executingTask.GetAwaiter().GetResult();
        }

        public static T Execute<T>(this Task<T> task , TaskExecutingOptions options = TaskExecutingOptions.CurrentThread)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }
            var executingTask = Task.Factory.StartNew(() => task , CancellationToken.None , TaskCreationOptions.None , GetTaskScheduler(options)).Unwrap();
            return executingTask.GetAwaiter().GetResult();
        }

        private static TaskScheduler GetTaskScheduler(TaskExecutingOptions options)
        {
            switch(options)
            {
                case TaskExecutingOptions.CurrentThread:
                    return TaskSchedulerManager<CurrentThreadTaskScheduler>.Default;
                case TaskExecutingOptions.ThreadPer:
                    return TaskSchedulerManager<ThreadPerTaskScheduler>.Default;
                default:
                    return TaskScheduler.Current;
            }
        }
    }
}
