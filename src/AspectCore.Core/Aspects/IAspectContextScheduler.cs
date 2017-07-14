using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    [NonAspect]
    public interface IAspectContextScheduler
    {
        bool TryEnter(ScopedAspectContext context);

        void Release(ScopedAspectContext context);

        bool TryInclude(ScopedAspectContext context, IInterceptor interceptor);

        AspectContext[] GetCurrentContexts();
    }
}
