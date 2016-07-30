using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions.Aspects
{
    public abstract class AspectContext
    {
        public AspectDelegate Next { get; set; }
        public ITarget Target { get; }
        public IProxy Proxy { get; }

        public AspectContext(ITarget target , IProxy proxy)
        {
            Target = target;
            Proxy = proxy;
        }
    }
}
