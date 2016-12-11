using AspectCore.Lite.Abstractions;
using AspectCore.Lite.DynamicProxy.Resolution.Generators;
using System;
using System.Linq;
using System.Reflection;

namespace AspectCore.Lite.DynamicProxy.Resolution
{
    public static class TypeInfoExtensions
    {
        public static bool ValidateAspect(this TypeInfo typeInfo, IAspectValidator aspectValidator)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            if (typeInfo.IsValueType)
            {
                return false;
            }

            return typeInfo.DeclaredMethods.Any(method => aspectValidator.Validate(method));
        }

        public static bool CanInherited(this TypeInfo typeInfo)
        {
            return typeInfo.IsClass && (typeInfo.IsPublic || (typeInfo.IsNested && typeInfo.IsNestedPublic)) &&
                   !typeInfo.IsSealed && !typeInfo.IsGenericTypeDefinition;
        }

        public static TypeInfo CreateProxyTypeInfo(this Type serviceType, Type implementationType, IAspectValidator aspectValidator)
        {
            var typeGenerator = new AspectTypeGenerator(serviceType, implementationType, aspectValidator);
            return typeGenerator.CreateTypeInfo();
        }

        public static Type CreateProxyType(this Type serviceType, Type implementationType, IAspectValidator aspectValidator)
        {
            var typeGenerator = new AspectTypeGenerator(serviceType, implementationType, aspectValidator);
            return typeGenerator.CreateType();
        }
    }
}
