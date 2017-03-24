using System.Collections.Generic;

namespace AspectCore.Abstractions
{
    public interface IAspectContextItemProvider
    {
        IDictionary<string, object> Items { get; }
    }
}
