// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
    internal static class CollectionExtensions
    {
#if NETSTANDARD2_0
        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            return dictionary.TryGetValue(key, out var obj)
                ? obj
                : defaultValue;
        }
#endif
    }
}
