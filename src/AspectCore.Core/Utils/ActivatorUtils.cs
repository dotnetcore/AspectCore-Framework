using System;
using AspectCore.DependencyInjection;

namespace AspectCore.Utils
{
    internal static class ActivatorUtils
    {
        /// <summary>
        /// 创建类型ManyEnumerable<elementType>的对象,其中只含有一个元素
        /// </summary>
        /// <param name="elementType">元素类型</param>
        /// <returns>类型ManyEnumerable<elementType>的对象</returns>
        public static object CreateManyEnumerable(Type elementType)
        {
            return Activator.CreateInstance(typeof(ManyEnumerable<>).MakeGenericType(elementType), Array.CreateInstance(elementType, 0));
        }

        /// <summary>
        /// 指定一个数组，创建类型ManyEnumerable<elementType>的对象
        /// </summary>
        /// <param name="elementType">元素类型</param>
        /// <param name="array">此数组传递给ManyEnumerable<elementType>的构造</param>
        /// <returns>类型ManyEnumerable<elementType>的对象</returns>
        public static object CreateManyEnumerable(Type elementType, Array array)
        {
            return Activator.CreateInstance(typeof(ManyEnumerable<>).MakeGenericType(elementType), array);
        }
    }
}