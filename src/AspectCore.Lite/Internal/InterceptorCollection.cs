using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Common;
using System.Reflection;

namespace AspectCore.Lite.Internal
{
    internal sealed class InterceptorCollection : IInterceptorCollection
    {
        private readonly ConcurrentBag<Func<IInterceptor>> interceptorConcurrentBag =
            new ConcurrentBag<Func<IInterceptor>>();

        public void Add(Type interceptorType, object[] args)
        {
            ExceptionHelper.ThrowArgumentNull(interceptorType, nameof(interceptorType));
            ExceptionHelper.ThrowArgument(() => interceptorType.GetTypeInfo().IsInstanceOfType(typeof(IInterceptor)), $"{interceptorType} not an interceptor type.", nameof(interceptorType));
            interceptorConcurrentBag.Add(() => (IInterceptor)Activator.CreateInstance(interceptorType, args));
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