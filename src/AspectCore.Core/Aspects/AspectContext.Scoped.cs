using System;
using System.Collections.Generic;
using System.Threading;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    public sealed class ScopedAspectContext : AspectContext
    {
        private readonly static AsyncLocal<AspectContext> CurrentContextLocal = new AsyncLocal<AspectContext>();
        private readonly IAspectContextScheduler _aspectContextScheduler;
        private readonly AspectContext _rtContext;

        internal int Id { get; set; }

        internal AspectContext RtContext
        {
            get
            {
                return CurrentContextLocal.Value;
            }
            private set
            {
                CurrentContextLocal.Value = value;
            }
        }

        public override IServiceProvider ServiceProvider => _rtContext.ServiceProvider;

        public override ITargetDescriptor Target => _rtContext.Target;

        public override IProxyDescriptor Proxy => _rtContext.Proxy;

        public override IParameterCollection Parameters => _rtContext.Parameters;

        public override IParameterDescriptor ReturnParameter => _rtContext.ReturnParameter;

        public override IDictionary<string, object> Items => _rtContext.Items;

        internal ScopedAspectContext(AspectContext aspectContext, IAspectContextScheduler aspectContextScheduler)
        {
            _rtContext = aspectContext;
            _aspectContextScheduler = aspectContextScheduler;
            if (!_aspectContextScheduler.TryEnter(this))
            {
                throw new InvalidOperationException("Error occurred in the schedule AspectContext.");
            }
            RtContext = _rtContext;
        }

        protected override void Dispose(bool disposing)
        {
            RtContext.Dispose();
            _aspectContextScheduler.Release(this);
        }
    }
}