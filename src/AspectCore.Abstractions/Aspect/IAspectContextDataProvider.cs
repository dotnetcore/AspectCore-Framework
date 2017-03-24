using System.Collections.Generic;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IAspectContextDataProvider
    {
        IDictionary<string, object> Items { get; }
    }
}
