using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions.Aspects
{
    public abstract class AspectContext
    {
        public AspectDelegate Next { get; set; }
        public ITarget Target { get; set; }
        public IProxy Proxy { get; set; }
    }
}
