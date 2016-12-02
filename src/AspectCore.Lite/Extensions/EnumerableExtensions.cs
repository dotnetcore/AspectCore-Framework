using AspectCore.Lite.Common;
using System;
using System.Collections.Generic;

namespace AspectCore.Lite.Extensions
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            ExceptionHelper.ThrowArgumentNull(source , nameof(source));
            ExceptionHelper.ThrowArgumentNull(action , nameof(action));

            foreach (T item in source) action(item);
            return source;
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            ExceptionHelper.ThrowArgumentNull(source , nameof(source));
            ExceptionHelper.ThrowArgumentNull(action , nameof(action));

            int index = 0;
            foreach (T item in source) action(item, index++);
            return source;
        }

        public static IEnumerable<TSource> Distinct<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            ExceptionHelper.ThrowArgumentNull(source , nameof(source));
            ExceptionHelper.ThrowArgumentNull(keySelector , nameof(keySelector));

            HashSet<TKey> set = new HashSet<TKey>();
            foreach (TSource item in source)
                if (set.Add(keySelector(item)))
                    yield return item;
        }

        public static IEnumerable<TSource> Distinct<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, bool> predicate)
        {
            ExceptionHelper.ThrowArgumentNull(source , nameof(source));
            ExceptionHelper.ThrowArgumentNull(keySelector , nameof(keySelector));
            ExceptionHelper.ThrowArgumentNull(predicate , nameof(predicate));

            HashSet<TKey> set = new HashSet<TKey>();
            foreach (TSource item in source)
            {
                if (predicate(item)) if (set.Add(keySelector(item)) == false) continue;
                yield return item;
            }
        }
    }
}
