using System;
using System.Collections.Generic;
using System.Text;

namespace AspectCore.Extensions.Reflection
{
    /// <summary>
    /// modify since <see cref="https://github.com/zkweb-framework/ZKWeb/blob/master/ZKWeb/ZKWebStandard/Collections/Pair.cs"/>
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    internal struct Pair<T1, T2> : IEquatable<Pair<T1, T2>>
    {
        /// </summary>
        public T1 Item1 { get; private set; }

        public T2 Item2 { get; private set; }

        public Pair(T1 first, T2 second)
        {
            Item1 = first;
            Item2 = second;
        }

        public bool Equals(Pair<T1, T2> obj)
        {
            return this.Item1.Equals(obj.Item1) && Item2.Equals(obj.Item2);
        }

        public override bool Equals(object obj)
        {
            return (obj is Pair<T1, T2>) && Equals((Pair<T1, T2>)obj);
        }

        public override int GetHashCode()
        {
            // same with Tuple.CombineHashCodess
            var hash_1 = this.Item1?.GetHashCode() ?? 0;
            var hash_2 = Item2?.GetHashCode() ?? 0;
            return (hash_1 << 5) + hash_1 ^ hash_2;
        }

        public override string ToString()
        {
            return $"({this.Item1?.ToString() ?? "null"}, {Item2.ToString() ?? "null"})";
        }

        public void Deconstruct(out T1 first, out T2 second)
        {
            first = this.Item1;
            second = Item2;
        }
    }

    internal static class Pair
    {
        public static Pair<T1,T2> Create<T1,T2>(T1 item1,T2 item2)
        {
            return new Pair<T1, T2>(item1, item2);
        }
    }
}
