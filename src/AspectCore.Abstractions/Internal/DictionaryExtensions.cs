using System;
using System.Collections.Generic;

namespace AspectCore.Abstractions.Internal
{
    internal static class DictionaryExtensions
    {
        internal static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> factory)
        {
            var value = default(TValue);

            if (dictionary.TryGetValue(key, out value))
            {
                return value;
            }

            lock (dictionary)
            {
                if (dictionary.TryGetValue(key, out value))
                {
                    return value;
                }

                value = factory(key);
                dictionary.Add(key, value);
                return value;
            }
        }
    }
}
