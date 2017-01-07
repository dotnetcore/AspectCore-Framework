using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions.Resolution
{
    public sealed class AspectBuilder : IAspectBuilder
    {
        private readonly IList<Func<AspectDelegate, AspectDelegate>> delegates;

        public AspectBuilder()
        {
            delegates = new List<Func<AspectDelegate, AspectDelegate>>();
        }

        public void AddAspectDelegate(Func<IAspectContext, AspectDelegate, Task> interceptorInvoke)
        {
            if (interceptorInvoke == null)
            {
                throw new ArgumentNullException(nameof(interceptorInvoke));
            }
            delegates.Add(next => context => interceptorInvoke(context, next));
        }

        public AspectDelegate Build(Func<object> targetInvoke)
        {
            if (targetInvoke == null)
            {
                throw new ArgumentNullException(nameof(targetInvoke));
            }
            AspectDelegate invoke = context =>
            {
                context.ReturnParameter.Value = targetInvoke();
                return Task.FromResult(0);
            };
            foreach (var next in delegates.Reverse())
            {
                invoke = next(invoke);
            }
            return invoke;
        }
    }
}
