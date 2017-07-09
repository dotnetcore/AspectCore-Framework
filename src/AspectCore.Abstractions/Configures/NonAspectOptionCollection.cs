using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public sealed class NonAspectOptionCollection : Collection<NonAspectOptions>, IEnumerable<NonAspectOptions>, IEnumerable
    {
    }
}
