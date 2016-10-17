using AspectCore.Lite.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            ExceptionUtilities.ThrowArgumentNull(source , nameof(source));
            ExceptionUtilities.ThrowArgumentNull(action , nameof(action));

            foreach (T item in source) action(item);
            return source;
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            ExceptionUtilities.ThrowArgumentNull(source , nameof(source));
            ExceptionUtilities.ThrowArgumentNull(action , nameof(action));

            int index = 0;
            foreach (T item in source) action(item, index++);
            return source;
        }

        public static IEnumerable<TSource> Distinct<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            ExceptionUtilities.ThrowArgumentNull(source , nameof(source));
            ExceptionUtilities.ThrowArgumentNull(keySelector , nameof(keySelector));

            HashSet<TKey> set = new HashSet<TKey>();
            foreach (TSource item in source)
                if (set.Add(keySelector(item)))
                    yield return item;
        }

        public static IEnumerable<TSource> Distinct<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, bool> predicate)
        {
            ExceptionUtilities.ThrowArgumentNull(source , nameof(source));
            ExceptionUtilities.ThrowArgumentNull(keySelector , nameof(keySelector));
            ExceptionUtilities.ThrowArgumentNull(predicate , nameof(predicate));

            HashSet<TKey> set = new HashSet<TKey>();
            foreach (TSource item in source)
            {
                if (predicate(item)) if (set.Add(keySelector(item)) == false) continue;
                yield return item;
            }
        }
    }
}
