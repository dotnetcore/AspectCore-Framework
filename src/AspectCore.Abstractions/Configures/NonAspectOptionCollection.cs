using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AspectCore.Abstractions
{
    public sealed class NonAspectOptionCollection : Collection<NonAspectOptions>, IEnumerable<NonAspectOptions>, IEnumerable
    {
    }
}
