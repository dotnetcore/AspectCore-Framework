using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AspectCore.Abstractions;
using AspectCore.Core.Internal;

namespace AspectCore.Core
{
    [NonAspect]
    public sealed class AspectBuilder : IAspectBuilder
    {
        private readonly IList<Func<AspectDelegate, AspectDelegate>> _delegates;

        public AspectBuilder()
        {
            _delegates = new List<Func<AspectDelegate, AspectDelegate>>();
        }

        public void AddAspectDelegate(Func<AspectContext, AspectDelegate, Task> interceptorInvoke)
        {
            if (interceptorInvoke == null)
            {
                throw new ArgumentNullException(nameof(interceptorInvoke));
            }
            _delegates.Add(next => context => interceptorInvoke(context, next));
        }

        public Func<Func<object>, AspectDelegate> Build()
        {
            return targetInvoke => Build(targetInvoke);
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
                return TaskCache.CompletedTask;
            };
            var count = _delegates.Count;
            for (var i = count - 1; i > -1; i--)
            {
                invoke = _delegates[i](invoke);
            }
            return invoke;
        }
    }
}
