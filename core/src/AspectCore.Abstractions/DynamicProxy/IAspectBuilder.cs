using System;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public interface IAspectBuilder
    {
        AspectDelegate Build();
    }
}
