using System;
using System.Threading.Tasks;
using AspectCore.Abstractions;

namespace AspectCore.Core.Configuration
{
    public class DelegateInterceptor : IInterceptor
    {
        private readonly Func<AspectDelegate, AspectDelegate> _aspectDelegate;

        public bool AllowMultiple => true;

        public bool Inherited { get; set; } = false;

        public int Order { get; set; }

        public DelegateInterceptor(Func<AspectDelegate, AspectDelegate> aspectDelegate, int order = 0)
        {
            _aspectDelegate = aspectDelegate ?? throw new ArgumentNullException(nameof(aspectDelegate));
            Order = order;
        }

        public Task Invoke(AspectContext context, AspectDelegate next)
        {
            return _aspectDelegate(next)(context);
        }
    }
}