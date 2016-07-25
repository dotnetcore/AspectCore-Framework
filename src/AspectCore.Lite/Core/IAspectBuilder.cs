using AspectCore.Lite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore
{
    public interface IAspectBuilder
    {
        AspectDelegate Build();
    }
}
