using System;
using System.Collections.Generic;

namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface IInterceptorTable : IEnumerable<IInterceptor>
    {
        void Add(Type interceptorType, params object[] args);
    }
}
