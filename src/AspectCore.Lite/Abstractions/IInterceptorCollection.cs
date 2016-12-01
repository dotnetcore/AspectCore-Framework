using System;
using System.Collections.Generic;

namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface IInterceptorCollection : IEnumerable<IInterceptor>
    {
        void Add(Type interceptorType, object[] args);
    }
}