using AspectCore.Lite.Abstractions.Resolution.Generators;
using System;
using System.Linq;
using System.Reflection;

namespace AspectCore.Lite.Abstractions.Resolution.Common
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
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }

            if (!typeInfo.IsClass || typeInfo.IsSealed)
            {
                return false;
            }

            if (typeInfo.IsDefined(typeof(NonAspectAttribute)))
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

        public static TypeInfo CreateProxyTypeInfo(this Type serviceType, Type implementationType, IAspectValidator aspectValidator)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }
            if (implementationType == null)
            {
                throw new ArgumentNullException(nameof(implementationType));
            }
            if (aspectValidator == null)
            {
                throw new ArgumentNullException(nameof(aspectValidator));
            }
            var typeGenerator = new AspectTypeGenerator(serviceType, implementationType, aspectValidator);
            return typeGenerator.CreateTypeInfo();
        }

        public static Type CreateProxyType(this Type serviceType, Type implementationType, IAspectValidator aspectValidator)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }
            if (implementationType == null)
            {
                throw new ArgumentNullException(nameof(implementationType));
            }
            if (aspectValidator == null)
            {
                throw new ArgumentNullException(nameof(aspectValidator));
            }
            var typeGenerator = new AspectTypeGenerator(serviceType, implementationType, aspectValidator);
            return typeGenerator.CreateType();
        }
    }
}
