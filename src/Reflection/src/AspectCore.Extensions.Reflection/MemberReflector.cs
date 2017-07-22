using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace AspectCore.Extensions.Reflection
{
    public abstract class MemberReflector<TMemberInfo> where TMemberInfo : MemberInfo
    {
        protected TMemberInfo _reflectionInfo;

        public virtual string Name => _reflectionInfo.Name;

        protected MemberReflector(TMemberInfo reflectionInfo)
        {
            if (reflectionInfo == null)
            {
                throw new ArgumentNullException(nameof(reflectionInfo));
            }
            _reflectionInfo = reflectionInfo;
        }

        /// <summary>
        /// find member using binary search
        /// </summary>
        /// <typeparam name="TReflector"></typeparam>
        /// <param name="reflectors">pre order by name</param>
        /// <param name="name"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected TReflector FindMember<TReflector, TMemberinfo>(TReflector[] reflectors, string name) where TReflector : MemberReflector<TMemberinfo> where TMemberinfo : MemberInfo
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