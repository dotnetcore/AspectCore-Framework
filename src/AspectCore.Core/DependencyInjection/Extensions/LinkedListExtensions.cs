using System;
using System.Collections.Generic;
using System.Text;

namespace AspectCore.DependencyInjection
{
    internal static class LinkedListExtensions
    {
        public static LinkedList<T> Add<T>(this LinkedList<T> linkedList, T value)
        {
            if (linkedList == null)
            {
                throw new ArgumentNullException(nameof(linkedList));
            }
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (linkedList.Count == 0) linkedList.AddFirst(value);
            else linkedList.AddAfter(linkedList.Last, value);
            return linkedList;
        }
    }
}
