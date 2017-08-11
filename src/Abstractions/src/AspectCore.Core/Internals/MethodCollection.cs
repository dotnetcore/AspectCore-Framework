using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace AspectCore.Core
{
    public static class MethodCollection
    {
        private static readonly Dictionary<MethodInfo, int> _indexCollections = new Dictionary<MethodInfo, int>();
        private static readonly Dictionary<int, MethodInfo> _methodcollections = new Dictionary<int, MethodInfo>();
        private static int _index = 0;

        internal static int Add(MethodInfo method)
        {
            lock (_indexCollections)
            {
                int index;
                if (_indexCollections.TryGetValue(method, out index))
                {
                    return index;
                }
                index = Interlocked.Increment(ref _index);
                _indexCollections.Add(method, index);
                _methodcollections.Add(index, method);
                return index;
            }
        }

        public static MethodInfo FindMethod(int index)
        {
            if (_methodcollections.TryGetValue(index, out MethodInfo value))
            {
                return value;
            }
            throw new InvalidOperationException($"Failed to find the method associated with the specified index '{index}'.");
        }
    }
}