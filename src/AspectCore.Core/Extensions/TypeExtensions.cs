#nullable enable
#pragma warning disable CA1859 // Use concrete types when possible for improved performance
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace AspectCore.Extensions;

[DebuggerDisplay("({OverriddenMethod.ReturnType.Name} {OverriddenMethod.Name}) -> ({CovariantReturnMethod.ReturnType.Name} {CovariantReturnMethod.Name})")]
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

        while (current.BaseType is not null)
        {
            depth++;
            current = current.BaseType;
        }

        return depth - 1; // 去掉 object 自己那一层
    }

    /// <summary>
    /// Determines whether the specified method is overridden by a covariant return method.
    /// </summary>
    /// <param name="method">The method that potentially has a covariant return type override.</param>
    /// <param name="covariantReturnMethod">The method that potentially overrides <paramref name="method"/> with a covariant return type.</param>
    /// <returns></returns>
    public static bool IsOverriddenByCovariantReturnMethod(this MethodInfo method, MethodInfo covariantReturnMethod)
    {
        if (covariantReturnMethod.IsInCovariantReturnChain() == false)
            return false;

        if (covariantReturnMethod.Name != method.Name)
            return false;

        if (method.DeclaringType is not { } dt1
            || covariantReturnMethod.DeclaringType is not { } dt2
            || dt1.IsCovariantReturnAssignableFrom(dt2) == false)
            return false;

        var genericParameterMap = CreateGenericParameterMap(method, covariantReturnMethod);
        var methodReturnType = method.ReturnType.SubstituteGenericParameters(genericParameterMap);

        // return types should not be the same.
        if (covariantReturnMethod.ReturnType == methodReturnType)
            return false;

        if (methodReturnType.IsCovariantReturnAssignableFrom(covariantReturnMethod.ReturnType) == false)
            return false;

        var params1 = covariantReturnMethod.GetParameters();
        var params2 = method.GetParameters();

        if (params1.Length != params2.Length)
            return false;

        foreach (var (p1, p2) in params1.Zip(params2))
        {
            var parameterType = p2.ParameterType.SubstituteGenericParameters(genericParameterMap);
            if (p1.ParameterType.IsCovariantReturnEquivalentTo(parameterType) == false)
                return false;
        }

        var isGeneric = covariantReturnMethod.IsGenericMethod;
        if (isGeneric != method.IsGenericMethod)
            return false;

        if (method.IsGenericMethodDefinition != covariantReturnMethod.IsGenericMethodDefinition)
            return false;

        if (isGeneric)
        {
            var args1 = covariantReturnMethod.GetGenericArguments();
            var args2 = method.GetGenericArguments();
            if (args1.Length != args2.Length)
                return false;

            foreach (var (a1, a2) in args1.Zip(args2))
            {
                if (a1.IsCovariantReturnEquivalentTo(a2.SubstituteGenericParameters(genericParameterMap)) == false)
                    return false;
            }
        }

        return true;
    }

    private static IReadOnlyDictionary<Type, Type> CreateGenericParameterMap(MethodInfo method, MethodInfo covariantReturnMethod)
    {
        var result = new Dictionary<Type, Type>();

        if (method.DeclaringType is { } declaringType && covariantReturnMethod.DeclaringType is { } covariantDeclaringType)
            AddTypeGenericParameterMap(declaringType, covariantDeclaringType, result);

        if (method.IsGenericMethod && covariantReturnMethod.IsGenericMethod)
        {
            var args1 = method.GetGenericArguments();
            var args2 = covariantReturnMethod.GetGenericArguments();
            foreach (var (a1, a2) in args1.Zip(args2))
            {
                if (a1.IsGenericParameter)
                    result[a1] = a2;
            }
        }

        return result;
    }

    private static void AddTypeGenericParameterMap(Type declaringType, Type covariantDeclaringType, Dictionary<Type, Type> result)
    {
        if (declaringType.IsGenericType == false)
            return;

        var projectedDeclaringType = FindMatchingBaseType(covariantDeclaringType, declaringType);
        if (projectedDeclaringType is null || projectedDeclaringType.IsGenericType == false)
            return;

        var genericParameters = declaringType.GetGenericTypeDefinition().GetGenericArguments();
        var genericArguments = projectedDeclaringType.GetGenericArguments();
        if (genericParameters.Length != genericArguments.Length)
            return;

        foreach (var (parameter, argument) in genericParameters.Zip(genericArguments))
        {
            result[parameter] = argument;
        }
    }

    private static Type? FindMatchingBaseType(Type type, Type declaringType)
    {
        var declaringTypeDefinition = declaringType.IsGenericType
            ? declaringType.GetGenericTypeDefinition()
            : declaringType;

        foreach (var candidate in EnumerateBaseTypesAndInterfaces(type))
        {
            if (candidate.IsGenericType)
            {
                if (candidate.GetGenericTypeDefinition() == declaringTypeDefinition)
                    return candidate;
            }
            else if (candidate == declaringTypeDefinition)
            {
                return candidate;
            }
        }

        return null;
    }

    private static IEnumerable<Type> EnumerateBaseTypesAndInterfaces(Type type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            yield return current;

            foreach (var @interface in current.GetInterfaces())
            {
                yield return @interface;
            }
        }
    }

    private static Type SubstituteGenericParameters(this Type type, IReadOnlyDictionary<Type, Type> genericParameterMap)
    {
        if (type.IsGenericParameter)
        {
            return genericParameterMap.GetValueOrDefault(type, type);
        }

        if (type.HasElementType)
        {
            var elementType = type.GetElementType()!.SubstituteGenericParameters(genericParameterMap);
            if (elementType == type.GetElementType())
                return type;

            if (type.IsArray)
                return type.GetArrayRank() == 1 ? elementType.MakeArrayType() : elementType.MakeArrayType(type.GetArrayRank());

            if (type.IsByRef)
                return elementType.MakeByRefType();

            if (type.IsPointer)
                return elementType.MakePointerType();

            return type;
        }

        if (type.IsGenericType && type.IsGenericTypeDefinition == false)
        {
            var args = type.GetGenericArguments();
            var substitutedArgs = args.Select(a => a.SubstituteGenericParameters(genericParameterMap)).ToArray();
            if (args.SequenceEqual(substitutedArgs))
                return type;

            return type.GetGenericTypeDefinition().MakeGenericType(substitutedArgs);
        }

        return type;
    }

    private static bool AreEquivalentGenericTypes(Type type, Type other, Func<Type, Type, bool> argumentComparer, Func<Type, Type, bool> typeDefinitionComparer)
    {
        if (type.IsArray && other.IsArray)
        {
            if (type.GetArrayRank() != other.GetArrayRank())
                return false;

            // ReSharper disable once TailRecursiveCall
            return argumentComparer(type.GetElementType()!, other.GetElementType()!);
        }

        if (type.IsGenericType == false || other.IsGenericType == false)
            return false;

        if (type.IsConstructedGenericType != other.IsConstructedGenericType)
            return false;

        if (type.IsGenericTypeDefinition != other.IsGenericTypeDefinition)
            return false;

        var args1 = type.GetGenericArguments();
        var args2 = other.GetGenericArguments();

        if (args1.Length != args2.Length)
            return false;

        var d1 = type.GetGenericTypeDefinition();
        var d2 = other.GetGenericTypeDefinition();

        if (typeDefinitionComparer(d1, d2) == false)
            return false;

        foreach (var (a1, a2) in args1.Zip(args2))
        {
            if (argumentComparer(a1, a2) == false)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether the specified generic parameter type is covariant.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the specified type is covariant; otherwise, <see langword="false"/>.</returns>
    public static bool IsGenericParameterCovariant(this Type type)
    {
        if (type.IsGenericParameter == false)
            return false;

        var variance = type.GenericParameterAttributes & GenericParameterAttributes.VarianceMask;
        return variance == GenericParameterAttributes.Covariant;
    }

    private static bool AreEquivalentGenericParameters(Type type, Type other)
    {
        if (type.IsGenericParameter == false || other.IsGenericParameter == false)
            return false;

        if (type.GenericParameterPosition != other.GenericParameterPosition)
            return false;

        if (type.DeclaringMethod != other.DeclaringMethod)
            return false;

        return type.DeclaringType == other.DeclaringType;
    }

    public static bool IsAssignableFromGenericTypeDefinition(this Type type, Type other)
    {
        if (type.IsGenericTypeDefinition == false)
            return false;

        var p = other;
        while (true)
        {
            if (type.IsAssignableFrom(p))
                return true;

            if (p.IsGenericTypeDefinition == false)
                return false;

            foreach (var it in p.GetInterfaces())
            {
                if (it.IsGenericType == false)
                    continue;

                if (type.IsAssignableFrom(it.GetGenericTypeDefinition()))
                    return true;
            }

            p = p.BaseType;

            if (p is null)
                break;

            p = p.GetGenericTypeDefinition();
        }

        return false;
    }

    private static bool TryUnwrapByRef(ref Type type, ref Type other)
    {
        if (type.IsByRef != other.IsByRef)
            return false;

        if (type.IsByRef == false)
            return true;

        if (other.IsByRef == false)
            return false;

        type = type.GetElementType()!;
        other = other.GetElementType()!;
        return true;
    }

    public static bool IsCovariantReturnAssignableFrom(this Type type, Type other)
    {
        if (TryUnwrapByRef(ref type, ref other) == false)
            return false;

        return type.IsAssignableFrom(other)
               || type.IsAssignableFromGenericTypeDefinition(other)
               || AreEquivalentGenericParameters(type, other)
               || AreEquivalentGenericTypes(type, other,
                   (a, b) => a.IsGenericParameterCovariant()
                       ? a.IsCovariantReturnAssignableFrom(b)
                       : a.IsCovariantReturnEquivalentTo(b),
                   (a, b) => a.IsAssignableFromGenericTypeDefinition(b));
    }

    public static bool IsCovariantReturnEquivalentTo(this Type type, Type other)
    {
        if (TryUnwrapByRef(ref type, ref other) == false)
            return false;

        return type == other
               || AreEquivalentGenericParameters(type, other)
               || AreEquivalentGenericTypes(type, other, IsCovariantReturnEquivalentTo, (a, b) => a == b);
    }
}
