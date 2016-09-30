using System.Collections.Generic;

namespace AspectCore.Lite.Abstractions
{
    public interface IAspectCollection : IEnumerable<Aspect>, IList<Aspect>
    {
    }
}
