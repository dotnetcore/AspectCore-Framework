using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace AspectCore.Extensions.Reflection
{
    internal struct IndexedLocalBuilder
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
