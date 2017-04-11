using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    public sealed class AspectConfigureOption<TOption> : IAspectConfigureOption<TOption>
    {
        private readonly ICollection<Func<MethodInfo, TOption>> _collection = new List<Func<MethodInfo, TOption>>();

        public void Add(Func<MethodInfo, TOption> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }
            _collection.Add(configure);
        }

        public IEnumerator<Func<MethodInfo, TOption>> GetEnumerator()
        {
            foreach (var item in _collection.ToArray())
                yield return item;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
