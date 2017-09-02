using System;

namespace AspectCore.DynamicProxy
{
    public interface IAspectBuilder
    {
        AspectDelegate Build();
    }
}
