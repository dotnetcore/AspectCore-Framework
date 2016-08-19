using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Extensions
{
    internal static class ListExtensions
    {
        internal static IList<T> ReplaceAt<T>(this IList<T> list, int index, T item)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));

            if (index < 0 || index >= list.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            list.RemoveAt(index);
            list.Insert(index, item);

            return list;
        }
    }
}
