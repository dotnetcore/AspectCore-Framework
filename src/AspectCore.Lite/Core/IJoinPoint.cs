 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Core
{
    public interface IJoinPoint
    {
        Target Target { get; set; }

        Proxy Proxy { get; set; }

        void AddDelegate(Func<AspectDelegate, AspectDelegate> @delegate);

        AspectDelegate Build();
    }
}
