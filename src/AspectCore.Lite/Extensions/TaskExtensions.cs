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
        public static void WaitWithAsync(this Task task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }
            if (task.IsCompleted)
            {
                return;
            }
            if(task.IsCanceled)
            {
                throw new InvalidOperationException("Task has been canceled.");
            }
            if(task.IsFaulted)
            {
                throw task.Exception;
            }
            var executingTask = Task.Factory.StartNew(() => task, CancellationToken.None, TaskCreationOptions.None, TaskSchedulerManager<CurrentThreadTaskScheduler>.Default).Unwrap();
            executingTask.GetAwaiter().GetResult();
        }

        public static T WaitWithAsync<T>(this Task<T> task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }
            if (task.IsCompleted)
            {
                return task.Result;
            }
            if (task.IsCanceled)
            {
                throw new InvalidOperationException("Task has been canceled.");
            }
            if (task.IsFaulted)
            {
                throw task.Exception;
            }
            var executingTask = Task.Factory.StartNew(() => task, CancellationToken.None, TaskCreationOptions.None, TaskSchedulerManager<CurrentThreadTaskScheduler>.Default).Unwrap();
            return executingTask.GetAwaiter().GetResult();
        }
    }

    internal enum TaskExecutingOptions
    {
        None,
        CurrentThread,
        ThreadPer
    }
}
