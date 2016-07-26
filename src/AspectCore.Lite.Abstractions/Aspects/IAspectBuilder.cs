using AspectCore.Lite;
using AspectCore.Lite.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions.Aspects
{
    public interface IAspectBuilder
    {
        IAspect Build();
    }
}
