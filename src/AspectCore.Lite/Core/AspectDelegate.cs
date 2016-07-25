using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite
{
    public  delegate Task<IAspect> AspectDelegate(AspectContext aspectContext);
}
