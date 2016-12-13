using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.Lite.Abstractions.Resolution
{
    public sealed class InterceptorConfiguration : IInterceptorConfiguration
    {
        private readonly ConcurrentBag<Func<MethodInfo, IInterceptor>> configurations = new ConcurrentBag<Func<MethodInfo, IInterceptor>>();

        public void Configure(Func<MethodInfo, IInterceptor> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            configurations.Add(configure);
        }

        public IEnumerator<Func<MethodInfo, IInterceptor>> GetEnumerator()
        {
            foreach (var factory in configurations.ToArray())
            {
                yield return factory;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
