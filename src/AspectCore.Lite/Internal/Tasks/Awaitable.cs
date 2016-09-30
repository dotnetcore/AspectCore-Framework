using AspectCore.Lite.Abstractions.Tasks;
using System;
using System.Threading.Tasks;

namespace AspectCore.Lite.Internal.Tasks
{
    internal class Awaitable : IAwaitable
    {
        private readonly Func<Task> taskFactory;
        public Awaitable(Func<Task> taskFactory)
        {
            if (taskFactory == null)
            {
                throw new ArgumentNullException("taskFactory");
            }

            this.taskFactory = taskFactory;
        }

        public void AwaitResult()
        {
            using (var executeContext = new ExecuteSynchronizationContext())
            {
                executeContext.Post(async d =>
                {
                    using (executeContext.MessageLoopScoped())
                    {
                        var task = taskFactory();
                        if (task == null)
                        {
                            throw new InvalidOperationException("taskFactory must get effective results");
                        }
                        await task;
                    }
                } , null);
                executeContext.BeginMessageLoop();
            }
        }
    }

    internal class Awaitable<T> : IAwaitable<T>
    {
        private readonly Func<Task<T>> taskFactory;
        public Awaitable(Func<Task<T>> taskFactory)
        {
            if (taskFactory == null)
            {
                throw new ArgumentNullException("taskFactory");
            }

            this.taskFactory = taskFactory;
        }

        public T AwaitResult()
        {
            using (var executeContext = new ExecuteSynchronizationContext())
            {
                T result = default(T);
                executeContext.Post(async d =>
                {
                    using (executeContext.MessageLoopScoped())
                    {
                        var task = taskFactory();
                        if (task == null)
                        {
                            throw new InvalidOperationException("taskFactory must get effective results");
                        }
                        result = await task;
                    }
                } , null);
                executeContext.BeginMessageLoop();
                return result;
            }
        }
    }
}
