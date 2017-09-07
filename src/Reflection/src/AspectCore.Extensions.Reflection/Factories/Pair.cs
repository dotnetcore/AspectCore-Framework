using System;
using System.Collections.Generic;
using System.Text;

namespace AspectCore.Extensions.Reflection
{
    /// <summary>
    /// modify since <see cref="https://github.com/zkweb-framework/ZKWeb/blob/master/ZKWeb/ZKWebStandard/Collections/Pair.cs"/>
    /// </summary>
    /// <typeparam name="Member"></typeparam>
    /// <typeparam name="TCallOptions"></typeparam>
    internal struct Pair<Member, TCallOptions> : IEquatable<Pair<Member, TCallOptions>>
    {
        /// </summary>
        public Member MemberInfo { get; private set; }

        public TCallOptions CallOptions { get; private set; }

        public Pair(Member first, TCallOptions second)
        {
            MemberInfo = first;
            CallOptions = second;
        }

        public bool Equals(Pair<Member, TCallOptions> obj)
        {
            return this.MemberInfo.Equals(obj.MemberInfo) && CallOptions.Equals(obj.CallOptions);
        }

        public override bool Equals(object obj)
        {
            return (obj is Pair<Member, TCallOptions>) && Equals((Pair<Member, TCallOptions>)obj);
        }

        public override int GetHashCode()
        {
            // same with Tuple.CombineHashCodess
            var hash_1 = this.MemberInfo?.GetHashCode() ?? 0;
            var hash_2 = CallOptions?.GetHashCode() ?? 0;
            return (hash_1 << 5) + hash_1 ^ hash_2;
        }

        public override string ToString()
        {
            return $"({this.MemberInfo?.ToString() ?? "null"}, {CallOptions.ToString() ?? "null"})";
        }

        public void Deconstruct(out Member first, out TCallOptions second)
        {
            first = this.MemberInfo;
            second = CallOptions;
        }
    }
}
