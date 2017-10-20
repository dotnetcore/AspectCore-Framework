using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public sealed class AspectBuilder : IAspectBuilder
    {
        private readonly IList<Func<AspectDelegate, AspectDelegate>> _delegates;
        private readonly AspectDelegate _complete;
        private AspectDelegate _aspectDelegate;

        public AspectBuilder(AspectDelegate complete, IList<Func<AspectDelegate, AspectDelegate>> delegates)
        {
            _complete = complete ?? throw new ArgumentNullException(nameof(complete));
            _delegates = delegates ?? new List<Func<AspectDelegate, AspectDelegate>>();
        }

        public IEnumerable<Func<AspectDelegate, AspectDelegate>> Delegates => _delegates;

        public void AddAspectDelegate(Func<AspectContext, AspectDelegate, Task> interceptorInvoke)
        {
            if (interceptorInvoke == null)
            {
                throw new ArgumentNullException(nameof(interceptorInvoke));
            }
            _delegates.Add(next => context => interceptorInvoke(context, next));
        }

        public AspectDelegate Build()
        {
            if (_aspectDelegate != null)
            {
                return _aspectDelegate;
            }
            AspectDelegate invoke = _complete;
            var count = _delegates.Count;
            for (var i = count - 1; i > -1; i--)
            {
                invoke = _delegates[i](invoke);
            }
            return (_aspectDelegate = invoke);
        }
    }
}