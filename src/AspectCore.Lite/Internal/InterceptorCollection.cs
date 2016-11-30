using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Common;

namespace AspectCore.Lite.Internal
{
    internal sealed class InterceptorCollection : IInterceptorCollection
    {
        private readonly ConcurrentBag<Func<IInterceptor>> interceptorConcurrentBag =
            new ConcurrentBag<Func<IInterceptor>>();

        public void Add(Expression<Func<IInterceptor>> factory)
        {
            ExceptionHelper.ThrowArgumentNull(factory, nameof(factory));
            interceptorConcurrentBag.Add(factory.Compile());
        }

        public IEnumerator<IInterceptor> GetEnumerator()
        {
            var factoryCollection = interceptorConcurrentBag.ToArray();
            foreach (var factory in factoryCollection)
            {
                var interceptor = factory();
                if (interceptor != null) yield return interceptor;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}