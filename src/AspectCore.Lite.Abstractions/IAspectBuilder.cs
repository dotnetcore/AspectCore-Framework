using System;

namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface IAspectBuilder
    {
        void AddAspectDelegate(AspectDelegate aspectDelegate);

        AspectDelegate Build(Func<object> targetMedthodInvoke);
    }
}
