using System;
using System.Reflection.Emit;

namespace AspectCore.Extensions.Reflection.Emit
{
    /// <summary>
    /// 可索引的本地变量构建器
    /// </summary>
    public struct IndexedLocalBuilder
    {
        public LocalBuilder LocalBuilder { get; }
        
        /// <summary>
        /// 局部变量的类型
        /// </summary>
        public Type LocalType { get; }

        /// <summary>
        /// 索引
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// 局部变量的索引
        /// </summary>
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
