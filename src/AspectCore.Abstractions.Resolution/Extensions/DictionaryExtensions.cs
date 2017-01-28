using System;
using System.Collections.Generic;

namespace AspectCore.Abstractions.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> factory, object synchronization)
        {
            var value = default(TValue);

            if (dictionary.TryGetValue(key, out value))
            {
                return value;
            }

            lock (synchronization)
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
