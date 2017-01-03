using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions.Common
{
    public static class TypeExtensions
    {
        public static Type MakeDefType(this Type byRefType)
        {
            if (byRefType == null) throw new ArgumentNullException(nameof(byRefType));
            if (!byRefType.IsByRef) throw new ArgumentException($"Type {byRefType} is not passed by reference.");

            var assemblyQualifiedName = byRefType.AssemblyQualifiedName;
            var index = assemblyQualifiedName.IndexOf('&');
            assemblyQualifiedName = assemblyQualifiedName.Remove(index, 1);

            return byRefType.GetTypeInfo().Assembly.GetType(assemblyQualifiedName, true);
        }
    }
}
