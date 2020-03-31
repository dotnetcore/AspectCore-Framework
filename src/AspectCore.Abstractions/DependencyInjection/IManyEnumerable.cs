using System.Collections;
using System.Collections.Generic;
using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
{
    [NonAspect, NonCallback]
    public interface IManyEnumerable<out T> : IEnumerable<T>, IEnumerable
    {
    }
}