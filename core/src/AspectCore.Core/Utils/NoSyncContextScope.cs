using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspectCore.Core.Utils
{
    public static class NoSyncContextScope
    {
        // See: https://stackoverflow.com/questions/28305968/use-task-run-in-synchronous-method-to-avoid-deadlock-waiting-on-async-method
        public static IDisposable Enter()
        {
            var context = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);
            return new Disposable(context);
        }

        private struct Disposable : IDisposable
        {
            private readonly SynchronizationContext _synchronizationContext;

            public Disposable(SynchronizationContext synchronizationContext)
            {
                _synchronizationContext = synchronizationContext;
            }

            public void Dispose() =>
                SynchronizationContext.SetSynchronizationContext(_synchronizationContext);
        }

        public static void Run(Task task)
        {
            using (Enter())
            {
                task.GetAwaiter().GetResult();
            }
        }

        public static T Run<T>(Task<T> task)
        {
            using (Enter())
            {
                return task.GetAwaiter().GetResult();
            }
        }
    }
}
