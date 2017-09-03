using System.Collections;
using System.Collections.Generic;

namespace AspectCore.Injector
{
    public interface IManyEnumerable<out T> : IEnumerable<T>, IEnumerable
    {
    }
}