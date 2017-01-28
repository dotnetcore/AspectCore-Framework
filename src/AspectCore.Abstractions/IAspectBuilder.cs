using System;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IAspectBuilder
    {
        Func<Func<object>, AspectDelegate> Build();
    }
}
