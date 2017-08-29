using System;
using System.Collections.Generic;
using System.Text;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IAspectContextFactory
    {
        AspectContext CreateContext(AspectActivatorContext activatorContext);
    }
}
