using System;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IAspectBuilder
    {
        AspectDelegate Build();
    }
}
