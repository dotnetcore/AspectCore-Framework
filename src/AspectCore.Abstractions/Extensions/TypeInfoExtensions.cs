using System;
using System.Reflection;

namespace AspectCore.Abstractions.Extensions
{
    public static class TypeInfoExtensions
    {
        public static Type MakeDefType(this TypeInfo byRefTypeInfo)
        {
            if (byRefTypeInfo == null)
            {
                throw new ArgumentNullException(nameof(byRefTypeInfo));
            }
            if (!byRefTypeInfo.IsByRef)
            {
                throw new ArgumentException($"Type {byRefTypeInfo} is not passed by reference.");
            }

            var assemblyQualifiedName = byRefTypeInfo.AssemblyQualifiedName;
            var index = assemblyQualifiedName.IndexOf('&');
            assemblyQualifiedName = assemblyQualifiedName.Remove(index, 1);

            return byRefTypeInfo.Assembly.GetType(assemblyQualifiedName, true);
        }

        public static bool IsDynamically(this TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            return typeInfo.IsDefined(typeof(DynamicallyAttribute), false);
        }

        public static bool CanInherited(this TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }

            if (!typeInfo.IsClass || typeInfo.IsSealed)
            {
                return false;
            }

            if (typeInfo.IsDefined(typeof(NonAspectAttribute)) || typeInfo.IsDynamically())
            {
                return false;
            }

            if (typeInfo.IsNested)
            {
                return typeInfo.IsNestedPublic && typeInfo.DeclaringType.GetTypeInfo().IsPublic;
            }
            else
            {
                return typeInfo.IsPublic;
            }
        }
    }
}
