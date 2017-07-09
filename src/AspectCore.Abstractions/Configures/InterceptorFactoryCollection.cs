using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public sealed class InterceptorFactoryCollection : Collection<IInterceptorFactory>, IEnumerable<IInterceptorFactory>, IEnumerable
    {
    }
}
