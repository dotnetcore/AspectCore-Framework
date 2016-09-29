using System.Collections.Generic;

namespace AspectCore.Lite.Core
{
    public interface IAspectCollection : IEnumerable<Aspect>, IList<Aspect>
    {
    }
}
