using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspectCore.Abstractions.Resolution
{
    public sealed class AspectConfigureOption<TOption> : IAspectConfigureOption<TOption>
    {
        private readonly ICollection<Func<MethodInfo, TOption>> collection = new List<Func<MethodInfo, TOption>>();

        public void Add(Func<MethodInfo, TOption> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }
            collection.Add(configure);
        }

        public IEnumerator<Func<MethodInfo, TOption>> GetEnumerator()
        {
            foreach (var item in collection.ToArray())
                yield return item;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
