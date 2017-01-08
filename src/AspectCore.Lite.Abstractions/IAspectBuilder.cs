using System;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface IAspectBuilder
    {
        void AddAspectDelegate(Func<IAspectContext, AspectDelegate, Task> interceptorInvoke);

        AspectDelegate Build(Func<object> targetInvoke);
    }
}
