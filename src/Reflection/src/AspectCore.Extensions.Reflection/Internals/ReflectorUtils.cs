using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace AspectCore.Extensions.Reflection
{
    internal static class ReflectorCacheUtils<TMemberInfo, TReflector>
    {
        private readonly static ConcurrentDictionary<TMemberInfo, TReflector> dictionary = new ConcurrentDictionary<TMemberInfo, TReflector>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TReflector GetOrAdd(TMemberInfo key, Func<TMemberInfo, TReflector> factory)
        {
            return dictionary.GetOrAdd(key, k => factory(k));
        }
    }

    internal static class ReflectorFindUtils
    {
        /// <summary>
        /// find member using binary search
        /// </summary>
        /// <typeparam name="TReflector"></typeparam>
        /// <param name="reflectors">pre order by name</param>
        /// <param name="name"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TReflector FindMember<TReflector, TMemberInfo>(TReflector[] reflectors, string name) where TReflector : MemberReflector<TMemberInfo> where TMemberInfo : MemberInfo
        {
            if (name == null)
            {
                throw new ArgumentNullException(name);
            }
            var length = reflectors.Length;
            if (length == 0)
            {
                return null;
            }
            if (length == 1)
            {
                var reflector = reflectors[0];
                if (reflector.Name == name)
                {
                    return reflector;
                }
                return null;
            }
            // do binary search
            var first = 0;
            while (first <= length)
            {
                var middle = (first + length) / 2;
                var entry = reflectors[middle];
                var compareResult = string.CompareOrdinal(entry.Name, name);
                if (compareResult == 0)
                {
                    return entry;
                }
                else if (compareResult < 0)
                {
                    first = middle + 1;
                }
                else if (compareResult > 0)
                {
                    length = middle - 1;
                }
            }
            return null;
        }
    }
}
