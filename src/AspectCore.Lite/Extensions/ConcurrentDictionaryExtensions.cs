using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Extensions
{
    //internal static class ConcurrentDictionaryExtensions
    //{
    //    // From https://github.com/dotnet/corefx/issues/394#issuecomment-69494764
    //    // This lets us pass a state parameter allocation free GetOrAdd
    //    internal static TValue GetOrAdd<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory)
    //    {
    //        Debug.Assert(dictionary != null);
    //        Debug.Assert(key != null);
    //        Debug.Assert(valueFactory != null);

    //        while (true)
    //        {
    //            TValue value;
    //            if (dictionary.TryGetValue(key, out value))
    //            {
    //                return value;
    //            }

    //            value = valueFactory(key);
    //            if (dictionary.TryAdd(key, value))
    //            {
    //                return value;
    //            }
    //        }
    //    }
    //}
}
