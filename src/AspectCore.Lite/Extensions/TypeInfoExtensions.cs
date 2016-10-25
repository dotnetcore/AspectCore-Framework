using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Lite.Extensions
{
    public static class TypeInfoExtensions
    {

        public static bool CanProxy(this TypeInfo typeInfo, IServiceProvider provider)
        {
            ExceptionUtilities.ThrowArgumentNull(typeInfo, nameof(typeInfo));
            ExceptionUtilities.ThrowArgumentNull(provider, nameof(provider));

            if (typeInfo.IsValueType)
            {
                return false;
            }

            var pointcut = provider.GetRequiredService<IPointcut>();
            return typeInfo.DeclaredMethods.Any(method => pointcut.IsMatch(method));
        }
    }
}
