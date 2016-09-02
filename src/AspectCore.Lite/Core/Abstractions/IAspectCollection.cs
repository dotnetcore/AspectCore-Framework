 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Core
{
    public interface IAspectCollection : IEnumerable<Aspect>, IList<Aspect>
    {
    }
}
