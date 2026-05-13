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

        /// <summary>
        /// Determines whether the method itself is a covariant-return override method.
        /// </summary>
        /// <param name="method">
        /// The method to inspect.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the method is marked with
        /// <c>PreserveBaseOverridesAttribute</c>; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public static bool IsCovariantReturnMethod(this MethodInfo method)
        {
            return PreserveBaseOverridesAttribute != null
                   && method.IsDefined(PreserveBaseOverridesAttribute);
        }

        /// <summary>
        /// Determines whether the method participates in a covariant-return override chain.
        /// </summary>
        /// <param name="method">
        /// The method to inspect.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the method itself, or its base definition,
        /// is a covariant-return override method; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public static bool IsInCovariantReturnChain(this MethodInfo method)
        {
            if (method.IsCovariantReturnMethod())
                return true;

            return method.GetBaseDefinition()
                         .IsCovariantReturnMethod();
        }

        /// <summary>
        /// Determines whether two methods belong to the same virtual override chain
        /// by comparing their base definitions.
        /// </summary>
        /// <param name="method">
        /// The first method to compare.
        /// </param>
        /// <param name="other">
        /// The second method to compare.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if both methods have the same base definition;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsSameBaseDefinition(this MethodInfo method, MethodInfo other)
        {
            return method.GetBaseDefinition() == other.GetBaseDefinition();
        }
    }
}

