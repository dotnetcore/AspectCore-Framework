using AspectCore.Lite.Abstractions.Tasks;
using AspectCore.Lite.Internal.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Extensions
{
    public static class TaskExtensions
    {
        public static IAwaitable AsAwaitable(this Task task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            return new Awaitable(() => task);
        }

        public static IAwaitable<T> AsAwaitable<T>(this Task<T> task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            return new Awaitable<T>(() => task);
        }
    }
}
