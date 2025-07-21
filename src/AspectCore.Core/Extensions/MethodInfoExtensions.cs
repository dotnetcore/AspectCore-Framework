using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static AspectCore.Extensions.TypeExtensions;

// ReSharper disable once CheckNamespace
namespace AspectCore.Extensions
{
    internal static class MethodInfoExtensions
    {
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

        public static bool IsPreserveBaseOverride(this MethodInfo method, bool checkBase)
        {
            if (PreserveBaseOverridesAttribute is null)
                return false;

            if (method.IsDefined(PreserveBaseOverridesAttribute))
                return true;

            return checkBase && method.GetBaseDefinition().IsDefined(PreserveBaseOverridesAttribute);
        }

        public static bool IsSameBaseDefinition(this MethodInfo method, MethodInfo other)
        {
            return method.GetBaseDefinition() == other.GetBaseDefinition();
        }
    }
}

