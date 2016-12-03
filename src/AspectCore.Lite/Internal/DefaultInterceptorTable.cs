using AspectCore.Lite.Abstractions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.Lite.Internal
{
    internal sealed class DefaultInterceptorTable : IInterceptorTable
    {
        private readonly ConcurrentBag<Func<IInterceptor>> interceptorBag = new ConcurrentBag<Func<IInterceptor>>();

        public void Add(Type interceptorType, params object[] args)
        {
            if (interceptorType == null)
            {
                throw new ArgumentNullException(nameof(interceptorType));
            }
            if (!typeof(IInterceptor).GetTypeInfo().IsAssignableFrom(interceptorType))
            {
                throw new ArgumentException($"{interceptorType} not an interceptor type.", nameof(interceptorType));
            }
            interceptorBag.Add(() => (IInterceptor)Activator.CreateInstance(interceptorType, args));
        }

        public IEnumerator<IInterceptor> GetEnumerator()
        {
            foreach (var interceptorFactory in interceptorBag.ToArray())
            {
                var interceptor = interceptorFactory.Invoke();
                if (interceptor != null)
                {
                    yield return interceptor;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
