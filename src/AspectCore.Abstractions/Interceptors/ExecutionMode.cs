using System;
using System.Collections.Generic;
using System.Text;

namespace AspectCore.Abstractions
{
    public enum ExecutionMode
    {
        PerExecuted,

        PerNested,

        PerScope
    }
}