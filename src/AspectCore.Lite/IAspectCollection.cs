using AspectCore.Lite.Abstractions.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions
{
    public interface IAspectCollection : IEnumerable<IAspect>, IList<IAspect>
    {
    }
}
