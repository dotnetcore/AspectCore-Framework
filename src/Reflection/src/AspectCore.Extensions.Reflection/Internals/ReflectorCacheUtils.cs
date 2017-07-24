using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AspectCore.Extensions.Reflection
{
    internal static class ReflectorCacheUtils<TMemberInfo, TReflector>
    {
        private readonly static Dictionary<TMemberInfo, TReflector> dictionary = new Dictionary<TMemberInfo, TReflector>();
        private readonly static object lockObj = new object();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TReflector GetOrAdd(TMemberInfo key, Func<TMemberInfo, TReflector> factory)
        {
            TReflector reflector;
            if (dictionary.TryGetValue(key, out reflector))
            {
                return reflector;
            }
            lock (lockObj)
            {
                if (!dictionary.TryGetValue(key, out reflector))
                {
                    reflector = factory(key);
                    dictionary.Add(key, reflector);
                }
            }
            return reflector;
        }
    }
}
