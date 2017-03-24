using System.Collections.Generic;

namespace AspectCore.Abstractions
{
    public interface IAspectContextDataProvider
    {
        IDictionary<string, object> Items { get; }
    }
}
