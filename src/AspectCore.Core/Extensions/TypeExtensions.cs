#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace AspectCore.Extensions
{
    internal readonly struct CovariantReturnMethodInfo
    {
        /// <summary>
        /// The method that defines the covariant return type —
        /// i.e., the overriding method that returns a more derived type.
        /// </summary>
        public readonly MethodInfo CovariantReturnMethod;

        /// <summary>
        /// Gets the inheritance depth of the type that declares the <see cref="CovariantReturnMethod"/>.
        /// </summary>
        public readonly int InheritanceDepth;

        /// <summary>
        /// Gets the method that is overridden or implemented by <see cref="CovariantReturnMethod"/>.
        /// </summary>
        /// <remarks>
        /// This <see cref="MethodInfo"/> is **reflected from the derived type**, not necessarily
        /// the base definition returned by <see cref="MethodInfo.GetBaseDefinition()"/>.
        /// <para>
        /// In other words, it represents the version of the base or interface method as seen
        /// through the derived class’s reflection context, which may differ from the canonical
        /// base definition when covariant return types are involved.
        /// </para>
        /// </remarks>
        public readonly MethodInfo OverriddenMethod;

        /// <summary>
        /// The set of interface method declarations (if any)
        /// that are implemented by the <see cref="CovariantReturnMethod"/>.
        /// </summary>
        public readonly HashSet<MethodInfo> InterfaceDeclarations;

        public CovariantReturnMethodInfo(MethodInfo covariantReturnMethod, MethodInfo overriddenMethod, HashSet<MethodInfo> interfaceDeclarations)
        {
            InterfaceDeclarations = interfaceDeclarations;
            OverriddenMethod = overriddenMethod;
            CovariantReturnMethod = covariantReturnMethod;
            InheritanceDepth = covariantReturnMethod.DeclaringType.GetInheritanceDepth();
        }
    }

    internal static class TypeExtensions
    {
        public static readonly Type? PreserveBaseOverridesAttribute = Type.GetType("System.Runtime.CompilerServices.PreserveBaseOverridesAttribute", false);

        /// <summary>
        /// Finds methods participating in covariant-return overrides on the specified type
        /// and matches them with their non-covariant overridden methods.
        /// </summary>
        /// <param name="type">
        /// The type whose methods should be inspected.
        /// </param>
        /// <returns>
        /// A collection of <see cref="CovariantReturnMethodInfo"/> containing:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// The covariant-return method.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// The corresponding overridden method with the original return type.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// The interface methods implemented by the covariant-return method.
        /// </description>
        /// </item>
        /// </list>
        /// Returns an empty collection if the current runtime does not support
        /// covariant return types.
        /// </returns>
        public static IReadOnlyList<CovariantReturnMethodInfo> GetCovariantReturnMethods(this Type type)
        {
            var result = new List<CovariantReturnMethodInfo>();
            // No PreserveBaseOverridesAttribute means that the runtime does not support covariant return types.
            if (PreserveBaseOverridesAttribute is null)
                return result;

            var methods = type
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .GroupBy(m => m.IsInCovariantReturnChain())
                .ToDictionary(m => m.Key, m => m.ToArray());

            var covariantReturnMethods = methods.GetValueOrDefault(true, Array.Empty<MethodInfo>());
            var otherMethods = methods.GetValueOrDefault(false, Array.Empty<MethodInfo>());

            foreach (var covariantReturnMethod in covariantReturnMethods)
            {
                var overriddenMethod = otherMethods.FirstOrDefault(m => m.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
                if (overriddenMethod is null)
                    continue;

                var interfaceDeclarations = covariantReturnMethod.GetInterfaceDeclarations().ToHashSet();
                result.Add(new CovariantReturnMethodInfo(covariantReturnMethod, overriddenMethod, interfaceDeclarations));
            }

            return result;
        }

        /// <summary>
        /// Gets the inheritance depth of the specified type.
        /// </summary>
        /// <param name="type">The type whose inheritance depth is calculated.</param>
        /// <returns>
        /// The inheritance depth of <paramref name="type"/>.
        /// Returns 0 for <see cref="object"/>, 1 for a class that directly inherits from <see cref="object"/>,
        /// 2 for its derived class, and so on.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="type"/> is <see langword="null"/>.
        /// </exception>
        public static int GetInheritanceDepth(this Type? type)
        {
            if (type is null)
                return 0;

            var depth = 0;
            var current = type;

            while (current.BaseType != null)
            {
                depth++;
                current = current.BaseType;
            }

            return depth - 1; // 去掉 object 自己那一层
        }

        /// <summary>
        /// Determines whether the specified method is overridden by a covariant return method.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="covariantReturnMethod">Assumes it is already a covariant return method.</param>
        /// <returns></returns>
        public static bool IsOverriddenByCovariantReturnMethod(this MethodInfo method, MethodInfo covariantReturnMethod)
        {
            if (covariantReturnMethod.Name != method.Name)
                return false;

            if (method.IsConstructedGenericMethod())
            {
                if (covariantReturnMethod.IsConstructedGenericMethod() == false)
                    return false;

                method = method.GetGenericMethodDefinition();
                covariantReturnMethod = covariantReturnMethod.GetGenericMethodDefinition();
            }

            // return types should not be the same.
            if (covariantReturnMethod.ReturnType == method.ReturnType)
                return false;

            if (method.ReturnType.IsAssignableFrom(covariantReturnMethod.ReturnType) == false)
                return false;

            var params1 = covariantReturnMethod.GetParameters();
            var params2 = method.GetParameters();

            if (params1.Length != params2.Length)
                return false;

            foreach (var (p1, p2) in params1.Zip(params2))
            {
                var t1 = p1.ParameterType;
                var t2 = p2.ParameterType;

                if (t1 == t2)
                    continue;

                if (t1.IsGenericParameter == false || t2.IsGenericParameter == false)
                    return false;

                var m1 = t1.DeclaringMethod;
                var m2 = t2.DeclaringMethod;

                if (m1 is null && m2 is not null
                   || m1 is not null && m2 is null)
                    return false;

                if (t1.DeclaringMethod != t2.DeclaringMethod)
                    return false;

                if (t1.DeclaringType != t2.DeclaringType)
                    return false;

                if (t1.GenericParameterPosition != t2.GenericParameterPosition)
                    return false;
            }

            var isGeneric = covariantReturnMethod.IsGenericMethod;
            if (isGeneric != method.IsGenericMethod)
                return false;

            if (isGeneric)
            {
                var args1 = covariantReturnMethod.GetGenericArguments();
                var args2 = method.GetGenericArguments();
                if (args1.Length != args2.Length)
                    return false;

                foreach (var (a1, a2) in args1.Zip(args2))
                {
                    if (a1.GenericParameterPosition != a2.GenericParameterPosition)
                        return false;
                }
            }

            return true;
        }
    }
}
