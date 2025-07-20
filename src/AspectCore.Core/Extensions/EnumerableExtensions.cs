using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace System.Linq
{
    internal static class EnumerableExtensions
    {
#if NETSTANDARD2_0 || NETSTANDARD2_1
        public static IEnumerable<(TFirst First, TSecond Second)> Zip<TFirst, TSecond>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second)
        {
            return first.Zip(second, (f, s) => (f, s));
        }
#endif

#if NETSTANDARD2_0
        public static HashSet<TSource> ToHashSet<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer = null)
        {
            return new HashSet<TSource>(source, comparer);
        }
#endif
    }
}