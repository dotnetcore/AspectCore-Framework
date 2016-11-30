using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AspectCore.Lite.Abstractions
{
    public interface IInterceptorCollection : IEnumerable<IInterceptor>
    {
        void Add(Expression<Func<IInterceptor>> factory);
    }
}