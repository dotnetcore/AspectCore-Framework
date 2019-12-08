using System;
using AspectCore.DependencyInjection;

namespace AspectCore.Utils
{
    internal static class ActivatorUtils
    {
        public static object CreateManyEnumerable(Type elementType)
        {
            return Activator.CreateInstance(typeof(ManyEnumerable<>).MakeGenericType(elementType), Array.CreateInstance(elementType, 0));
        }

        public static object CreateManyEnumerable(Type elementType, Array array)
        {
            return Activator.CreateInstance(typeof(ManyEnumerable<>).MakeGenericType(elementType), array);
        }
    }
}