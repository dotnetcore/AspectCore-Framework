using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace AspectCore.Extensions
{
    internal readonly struct CovariantReturnMethodInfo
    {
        public readonly MethodInfo CovariantReturnMethod;
        public readonly MethodInfo OverridenMethod;
        public readonly HashSet<MethodInfo> InterfaceDeclarations;

        public CovariantReturnMethodInfo(MethodInfo covariantReturnMethod, MethodInfo overridenMethod, HashSet<MethodInfo> interfaceDeclarations)
        {
            InterfaceDeclarations = interfaceDeclarations;
            OverridenMethod = overridenMethod;
            CovariantReturnMethod = covariantReturnMethod;
        }
    }

    internal static class TypeExtensions
    {
        public static readonly Type PreserveBaseOverridesAttribute = Type.GetType("System.Runtime.CompilerServices.PreserveBaseOverridesAttribute", false);

        public static IReadOnlyList<CovariantReturnMethodInfo> GetCovariantReturnMethods(this Type type)
        {
            var result = new List<CovariantReturnMethodInfo>();
            // No PreserveBaseOverridesAttribute means that the runtime does not support covariant return types.
            if (PreserveBaseOverridesAttribute is null)
                return result;

            var methods = type
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .GroupBy(m => m.IsPreserveBaseOverride(true))
                .ToDictionary(m => m.Key, m => m.ToArray());

            var covariantReturnMethods = methods.GetValueOrDefault(true, Array.Empty<MethodInfo>());
            var otherMethods = methods.GetValueOrDefault(false, Array.Empty<MethodInfo>());

            foreach (var covariantReturnMethod in covariantReturnMethods)
            {
                var overridenMethod = otherMethods.FirstOrDefault(m => Match(covariantReturnMethod, m));
                if (overridenMethod is null)
                    continue;

                var interfaceDeclarations = covariantReturnMethod.GetInterfaceDeclarations().ToHashSet();
                result.Add(new CovariantReturnMethodInfo(covariantReturnMethod, overridenMethod, interfaceDeclarations));
            }

            return result;

            bool Match(MethodInfo covariantReturnMethod, MethodInfo other)
            {
                if (covariantReturnMethod.Name != other.Name)
                    return false;

                // return types should not be the same.
                if (covariantReturnMethod.ReturnType == other.ReturnType)
                    return false;

                if (other.ReturnType.IsAssignableFrom(covariantReturnMethod.ReturnType) == false)
                    return false;

                var params1 = covariantReturnMethod.GetParameters();
                var params2 = other.GetParameters();

                if (params1.Length != params2.Length)
                    return false;

                foreach (var (p1, p2) in params1.Zip(params2))
                {
                    if (p1.ParameterType != p2.ParameterType)
                        return false;
                }

                var isGeneric = covariantReturnMethod.IsGenericMethod;
                if (isGeneric != other.IsGenericMethod)
                    return false;

                if (isGeneric)
                {
                    var args1 = covariantReturnMethod.GetGenericArguments();
                    var args2 = other.GetGenericArguments();
                    if (args1.Length != args2.Length)
                        return false;

                    foreach (var (a1, a2) in args1.Zip(args2))
                    {
                        if (a1 != a2)
                            return false;
                    }
                }

                return true;
            }
        }
    }
}
