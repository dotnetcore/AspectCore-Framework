using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using AspectCore.Lite.Abstractions.Resolution.Utils;

namespace AspectCore.Lite.Abstractions.Resolution
{
    public sealed class AspectConfigurator : IAspectConfigurator
    {
        private readonly ConcurrentBag<Func<MethodInfo, IInterceptor>> configurations = new ConcurrentBag<Func<MethodInfo, IInterceptor>>();
        private readonly ConcurrentBag<Func<MethodInfo, bool>> ignoreConfigurations = new ConcurrentBag<Func<MethodInfo, bool>>();

        public AspectConfigurator()
        {
            Ignore(method => method.DeclaringType.FullName.Match("Microsoft.AspNetCore.*"));
            Ignore(method => method.DeclaringType.FullName.Match("Microsoft.Extensions.*"));
            Ignore(method => method.DeclaringType.FullName.Match("Microsoft.ApplicationInsights.*"));
            Ignore(method => method.DeclaringType.FullName.Match("Microsoft.Net.*"));
            Ignore(method => method.DeclaringType.FullName.Match("System.*"));
            Ignore(method => method.Name.Match("Equals"));
            Ignore(method => method.Name.Match("GetHashCode"));
            Ignore(method => method.Name.Match("ToString"));
        }

        public void Add(Func<MethodInfo, IInterceptor> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            configurations.Add(configure);
        }

        IEnumerator<Func<MethodInfo, IInterceptor>> IEnumerable<Func<MethodInfo, IInterceptor>>.GetEnumerator()
        {
            foreach (var factory in configurations.ToArray())
            {
                yield return factory;
            }
        }

        public void Ignore(Func<MethodInfo, bool> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            ignoreConfigurations.Add(configure);
        }

        IEnumerator<Func<MethodInfo, bool>> IEnumerable<Func<MethodInfo, bool>>.GetEnumerator()
        {
            foreach (var factory in ignoreConfigurations.ToArray())
            {
                yield return factory;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
