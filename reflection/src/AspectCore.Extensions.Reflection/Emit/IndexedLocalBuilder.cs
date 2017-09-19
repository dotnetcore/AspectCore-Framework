using System;
using System.Reflection.Emit;

namespace AspectCore.Extensions.Reflection.Emit
{
    public struct IndexedLocalBuilder
    {
        public LocalBuilder LocalBuilder { get; }   
        public Type LocalType { get; }
        public int Index { get; }
        public int LocalIndex { get; }

        public IndexedLocalBuilder(LocalBuilder localBuilder, int index)
        {
            LocalBuilder = localBuilder;
            LocalType = localBuilder.LocalType;
            LocalIndex = localBuilder.LocalIndex;
            Index = index;
        }
    }
}
