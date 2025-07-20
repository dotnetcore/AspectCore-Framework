using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Extensions;

// ReSharper disable once CheckNamespace
namespace AspectCore.Utils
{
    // NOTE: 
    // For class proxy: We just define the covariant return methods in the implementation type like normal methods, the CLR will handle the propagation.
    // For interface proxy: We need to use the covariant return methods as the interface methods' implementation.
    internal partial class ProxyGeneratorUtils
    {
        private static readonly Type PreserveBaseOverridesAttribute = Type.GetType("System.Runtime.CompilerServices.PreserveBaseOverridesAttribute", false);

        // key: covariant return method
        // value: interface method declarations
        internal static IReadOnlyDictionary<MethodInfo, HashSet<MethodInfo>> GetCovariantReturnMethodMap(Type implType)
        {
            var result = new Dictionary<MethodInfo, HashSet<MethodInfo>>();
            // No PreserveBaseOverridesAttribute means that the runtime does not support covariant return types.
            if (PreserveBaseOverridesAttribute is null)
                return result;

            const MethodAttributes attributes = MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot;
            var covariantReturnMethods = implType
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => (m.Attributes & attributes) == attributes)
                .Where(m => m.IsDefined(PreserveBaseOverridesAttribute))
                .ToHashSet();

            foreach (var method in covariantReturnMethods)
            {
                var interfaceDeclarations = method.GetInterfaceDeclarations().ToHashSet();
                result[method] = interfaceDeclarations;
            }

            return result;
        }
    }
}