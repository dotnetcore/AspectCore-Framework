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
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (action == null) throw new ArgumentNullException(nameof(action));
            foreach (T item in source) action(item);
            return source;
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (action == null) throw new ArgumentNullException(nameof(action));
            int index = 0;
            foreach (T item in source) action(item, index++);
            return source;
        }

        public static IEnumerable<TSource> Distinct<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            HashSet<TKey> set = new HashSet<TKey>();
            foreach (TSource item in source)
                if (set.Add(keySelector(item)))
                    yield return item;
        }

        public static IEnumerable<TSource> Distinct<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, bool> predicate)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            HashSet<TKey> set = new HashSet<TKey>();
            foreach (TSource item in source)
            {
                if (predicate(item)) if (set.Add(keySelector(item)) == false) continue;
                yield return item;
            }
        }


        //public static void For<T>(T index, Predicate<T> predicate, Func<T, T> func, Action<T> action)
        //{
        //    for (T i = index; predicate(i); i = func(i))
        //    {
        //        action(i);
        //    }
        //}

        //public static void AA()
        //{
        //    For(0, i => i < 10, i => i + 1, i => Console.WriteLine(i));
        //}
    }
}
