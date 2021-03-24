using System;
using System.Collections.Generic;
using System.Text;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// LinkedList扩展
    /// </summary>
    internal static class LinkedListExtensions
    {
        /// <summary>
        /// 向LinkedList末尾添加节点元素
        /// </summary>
        /// <typeparam name="T">节点类型</typeparam>
        /// <param name="linkedList">链表</param>
        /// <param name="value">值</param>
        /// <returns>链表</returns>
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
