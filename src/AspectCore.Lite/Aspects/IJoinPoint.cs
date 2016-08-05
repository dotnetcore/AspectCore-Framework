using AspectCore.Lite.Abstractions.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Aspects
{
    public interface IJoinPoint
    {
        ITarget Target { get; set; }

        IProxy Proxy { get; set; }

        void AddDelegate(Func<AspectDelegate, AspectDelegate> @delegate);

        AspectDelegate Build();
    }
}
