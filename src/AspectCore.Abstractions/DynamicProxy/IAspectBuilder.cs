using System;
using System.Collections.Generic;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public interface IAspectBuilder
    {
        IEnumerable<Func<AspectDelegate, AspectDelegate>> Delegates { get; }

        AspectDelegate Build();
    }
}
