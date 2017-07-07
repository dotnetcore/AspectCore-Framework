using System;
using System.Collections.Generic;
using System.Text;

namespace AspectCore.Abstractions
{
    public interface ISortableInterceptor
    {
        int Order { get; set; }
    }
}