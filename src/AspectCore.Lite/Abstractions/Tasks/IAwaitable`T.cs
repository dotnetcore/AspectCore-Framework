using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions.Tasks
{
    public interface IAwaitable<T>
    {
        T AwaitResult();
    }
}
