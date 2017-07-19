using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.ScopedContext
{
    [NonAspect]
    public interface IAspectContextScheduler
    {
        bool TryEnter(AspectContext context);

        void Release(AspectContext context);

        bool TryInclude(AspectContext context, IInterceptor interceptor);

        AspectContext[] GetCurrentContexts();
    }
}
