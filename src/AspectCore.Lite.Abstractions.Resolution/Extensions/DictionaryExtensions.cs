using System;
using System.Collections.Generic;

namespace AspectCore.Lite.Abstractions.Extensions
{
    internal static class DictionaryExtensions
    {
        internal static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> factory, object synchronization)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }
            if (synchronization == null)
            {
                throw new ArgumentNullException(nameof(synchronization));
            }

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
