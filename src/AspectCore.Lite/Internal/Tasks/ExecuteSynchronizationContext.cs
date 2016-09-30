using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspectCore.Lite.Internal.Tasks
{
    internal sealed class ExecuteSynchronizationContext : SynchronizationContext, IDisposable
    {
        private bool done;
        private readonly AutoResetEvent autoResetEvent = new AutoResetEvent(false);
        private readonly ConcurrentQueue<SendOrPostCallbackEntry> sendOrPostCallbackEntryQueue = new ConcurrentQueue<SendOrPostCallbackEntry>();
        private readonly SynchronizationContext currentSynchronizationContext;

        public ExecuteSynchronizationContext()
        {
            currentSynchronizationContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(this);
        }

        public override void Send(SendOrPostCallback d , object state)
        {
            throw new NotSupportedException("cannot send to same thread");
        }

        public override void Post(SendOrPostCallback d , object state)
        {
            sendOrPostCallbackEntryQueue.Enqueue(new SendOrPostCallbackEntry(d , state));
            autoResetEvent.Set();
        }

        public override SynchronizationContext CreateCopy()
        {
            return this;
        }

        public void BeginMessageLoop()
        {
            while (!done)
            {
                SendOrPostCallbackEntry entry = default(SendOrPostCallbackEntry);
                if (sendOrPostCallbackEntryQueue.TryDequeue(out entry))
                {
                    entry.Invoke();
                    break;
                }
                autoResetEvent.WaitOne();
            }
        }

        public void EndMessageLoop()
        {
            Post(d => done = true , null);
        }

        /// <summary>
        /// auto invoke EndMessageLoop
        /// </summary>
        /// <returns></returns>
        public IDisposable MessageLoopScoped()
        {
            return new Disposable(this);
        }

        public void Dispose()
        {
            autoResetEvent.Dispose();
            SynchronizationContext.SetSynchronizationContext(currentSynchronizationContext);
        }

        private sealed class Disposable : IDisposable
        {
            private readonly ExecuteSynchronizationContext context;

            public Disposable(ExecuteSynchronizationContext context)
            {
                this.context = context;
            }

            public void Dispose()
            {
                context.EndMessageLoop();
            }
        }

        private struct SendOrPostCallbackEntry
        {
            private readonly SendOrPostCallback callback;
            private readonly object state;

            public SendOrPostCallbackEntry(SendOrPostCallback callback , object state)
            {
                this.callback = callback;
                this.state = state;
            }

            public void Invoke()
            {
                callback.Invoke(state);
            }
        }
    }
}
