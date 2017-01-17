using System;
using System.Threading.Tasks;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IAspectBuilder
    {
        void AddAspectDelegate(Func<IAspectContext, AspectDelegate, Task> interceptorInvoke);

        AspectDelegate Build(Func<object> targetInvoke);
    }
}
