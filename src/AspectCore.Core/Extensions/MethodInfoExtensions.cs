using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace AspectCore.Extensions
{
    internal static class MethodInfoExtensions
    {
        public static readonly Type PreserveBaseOverridesAttribute = Type.GetType("System.Runtime.CompilerServices.PreserveBaseOverridesAttribute", false);

        public static IEnumerable<MethodInfo> GetInterfaceDeclarations(this MethodInfo method)
        {
            var typeInfo = method.ReflectedType?.GetTypeInfo();
            if (typeInfo is null)
                yield break;

            foreach (var implementedInterface in typeInfo.ImplementedInterfaces)
            {
                var map = typeInfo.GetInterfaceMap(implementedInterface);
                foreach (var (interfaceMethod, targetMethod) in map.InterfaceMethods.Zip(map.TargetMethods))
                {
                    if (targetMethod == method)
                        yield return interfaceMethod;
                }
            }
        }

        public static bool IsOverriden(this MethodInfo method)
        {
            return method.GetBaseDefinition() != method;
        }

        public static bool IsPreserveBaseOverride(this MethodInfo method, bool checkBase)
        {
            if (PreserveBaseOverridesAttribute is null)
                return false;

            var m = method;
            while (true)
            {
                if (m.IsDefined(PreserveBaseOverridesAttribute))
                    return true;

                if (checkBase == false)
                    break;

                var b = m.GetBaseDefinition();
                if (b == m || b == null)
                    break;

                m = b;
            }

            return false;
        }
    }
}

