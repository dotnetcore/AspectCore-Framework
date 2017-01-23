using System;
using System.Threading.Tasks;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IAspectBuilder
    {
        void AddAspectDelegate(Func<AspectContext, AspectDelegate, Task> interceptorInvoke);

        AspectDelegate Build(Func<object> targetInvoke);
    }
}
