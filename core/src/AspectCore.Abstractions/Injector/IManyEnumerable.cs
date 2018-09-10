using System.Collections;
using System.Collections.Generic;
using AspectCore.DynamicProxy;

namespace AspectCore.Injector
{
    [NonAspect, NonCallback]
    public interface IManyEnumerable<out T> : IEnumerable<T>, IEnumerable
    {
    }
}