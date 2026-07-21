// Copyright (c) AspectCore. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// Structural equality comparer for <see cref="Type"/> arrays.
    /// Used by source-generated proxies to cache MakeGenericMethod results
    /// keyed by type argument arrays.
    /// </summary>
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    public sealed class TypeArrayComparer : IEqualityComparer<Type[]>
    {
        public static readonly TypeArrayComparer Instance = new();

        public bool Equals(Type[]? x, Type[]? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null || x.Length != y.Length) return false;
            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i]) return false;
            }
            return true;
        }

        public int GetHashCode(Type[] obj)
        {
            var hash = new HashCode();
            foreach (var t in obj)
            {
                hash.Add(t);
            }
            return hash.ToHashCode();
        }
    }
}
