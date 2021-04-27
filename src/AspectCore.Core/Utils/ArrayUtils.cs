namespace AspectCore.Utils
{
    internal static class ArrayUtils
    {
        /// <summary>
        /// 提供一个类型为 T 的空数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] Empty<T>()
        {
            return EmptyArray<T>.Value;
        }

        private static class EmptyArray<T>
        {
            public static readonly T[] Value = new T[0];
        }
    }
}