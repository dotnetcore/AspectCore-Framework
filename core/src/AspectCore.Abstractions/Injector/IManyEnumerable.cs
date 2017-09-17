using System.Collections;
using System.Collections.Generic;
using AspectCore.DynamicProxy;

namespace AspectCore.Injector
{
    [NonAspect]
    public interface IManyEnumerable<out T> : IEnumerable<T>, IEnumerable
    {
    }
}