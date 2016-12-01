using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Common;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace AspectCore.Lite.Extensions
{
    public static class TypeInfoExtensions
    {
        public static bool CanProxy(this TypeInfo typeInfo, IServiceProvider provider)
        {
            ExceptionHelper.ThrowArgumentNull(typeInfo, nameof(typeInfo));
            ExceptionHelper.ThrowArgumentNull(provider, nameof(provider));

            if (typeInfo.IsValueType)
            {
                return false;
            }

            var pointcut = provider.GetRequiredService<IPointcut>();
            return typeInfo.DeclaredMethods.Any(method => pointcut.IsMatch(method));
        }

        public static bool CanInherited(this TypeInfo typeInfo)
        {
            ExceptionHelper.ThrowArgumentNull(typeInfo, nameof(typeInfo));
            return typeInfo.IsClass && (typeInfo.IsPublic || (typeInfo.IsNested && typeInfo.IsNestedPublic)) &&
                   !typeInfo.IsSealed && !typeInfo.IsGenericTypeDefinition;
        }
    }
}
