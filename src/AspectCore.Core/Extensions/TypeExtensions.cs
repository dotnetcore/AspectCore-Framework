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
    /// The method that defines the covariant return type,
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
    /// This <see cref="MethodInfo"/> is reflected from the derived type, not necessarily
    /// the base definition returned by <see cref="MethodInfo.GetBaseDefinition()"/>.
    /// <para>
    /// In other words, it represents the version of the base or interface method as seen
    /// through the derived class's reflection context, which may differ from the canonical
    /// base definition when covariant return types are involved.
    /// </para>
    /// </remarks>
    public readonly MethodInfo OverriddenMethod;

    /// <summary>
    /// The set of interface method declarations (if any)
    /// that are implemented by the <see cref="CovariantReturnMethod"/>.
    /// </summary>
    public readonly HashSet<MethodInfo> InterfaceDeclarations;

    /// <summary>
    /// Initializes a new instance of <see cref="CovariantReturnMethodInfo"/>.
    /// </summary>
    /// <param name="covariantReturnMethod">The overriding method whose return type is more specific.</param>
    /// <param name="overriddenMethod">The base or interface method matched to <paramref name="covariantReturnMethod"/>.</param>
    /// <param name="interfaceDeclarations">The interface declarations implemented by <paramref name="covariantReturnMethod"/>.</param>
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
    /// <summary>
    /// Gets the runtime marker attribute used by the CLR to preserve covariant-return base overrides.
    /// </summary>
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
    /// Gets the inheritance depth of the specified type, excluding the <see cref="object"/> level.
    /// </summary>
    /// <param name="type">The type whose inheritance depth is calculated.</param>
    /// <returns>
    /// The inheritance depth of <paramref name="type"/> after subtracting the <see cref="object"/> level.
    /// A type that directly inherits from <see cref="object"/> returns 0, and each derived class adds 1.
    /// </returns>
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

        return depth - 1;
    }

    /// <summary>
    /// Determines whether a method is overridden by another method through a covariant-return override.
    /// </summary>
    /// <param name="method">The base, interface, or less-derived method to match.</param>
    /// <param name="covariantReturnMethod">The candidate method that may override <paramref name="method"/> with a more specific return type.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="covariantReturnMethod"/> is a covariant-return override of
    /// <paramref name="method"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsOverriddenByCovariantReturnMethod(this MethodInfo method, MethodInfo covariantReturnMethod)
    {
        if (covariantReturnMethod.IsInCovariantReturnChain() == false)
            return false;

        if (covariantReturnMethod.Name != method.Name)
            return false;

        if (method.DeclaringType is not { } methodDeclaringType
            || covariantReturnMethod.DeclaringType is not { } covariantDeclaringType
            || methodDeclaringType.IsCovariantReturnAssignableFrom(covariantDeclaringType) == false)
            return false;

        var genericParameterMap = CreateGenericParameterMap(method, covariantReturnMethod);
        var methodReturnType = method.ReturnType.SubstituteGenericParameters(genericParameterMap);

        // Covariant return overrides must narrow the return type.
        if (covariantReturnMethod.ReturnType == methodReturnType)
            return false;

        if (methodReturnType.IsCovariantReturnAssignableFrom(covariantReturnMethod.ReturnType) == false)
            return false;

        var covariantParameters = covariantReturnMethod.GetParameters();
        var methodParameters = method.GetParameters();

        if (covariantParameters.Length != methodParameters.Length)
            return false;

        foreach (var (covariantParameter, methodParameter) in covariantParameters.Zip(methodParameters))
        {
            var parameterType = methodParameter.ParameterType.SubstituteGenericParameters(genericParameterMap);
            if (covariantParameter.ParameterType.IsCovariantReturnEquivalentTo(parameterType) == false)
                return false;
        }

        var isGeneric = covariantReturnMethod.IsGenericMethod;
        if (isGeneric != method.IsGenericMethod)
            return false;

        if (method.IsGenericMethodDefinition != covariantReturnMethod.IsGenericMethodDefinition)
            return false;

        if (isGeneric)
        {
            var covariantGenericArguments = covariantReturnMethod.GetGenericArguments();
            var methodGenericArguments = method.GetGenericArguments();
            if (covariantGenericArguments.Length != methodGenericArguments.Length)
                return false;

            foreach (var (covariantGenericArgument, methodGenericArgument) in covariantGenericArguments.Zip(methodGenericArguments))
            {
                if (covariantGenericArgument.IsCovariantReturnEquivalentTo(methodGenericArgument.SubstituteGenericParameters(genericParameterMap)) == false)
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Builds a map from generic parameters on the base method or declaring type to their matching parameters or arguments on the covariant method.
    /// </summary>
    /// <param name="method">The method whose generic parameters should be projected.</param>
    /// <param name="covariantReturnMethod">The candidate covariant-return method that supplies projected parameters or arguments.</param>
    /// <returns>A dictionary used to substitute generic parameters before comparing signatures.</returns>
    private static IReadOnlyDictionary<Type, Type> CreateGenericParameterMap(MethodInfo method, MethodInfo covariantReturnMethod)
    {
        var result = new Dictionary<Type, Type>();

        if (method.DeclaringType is { } declaringType && covariantReturnMethod.DeclaringType is { } covariantDeclaringType)
            AddTypeGenericParameterMap(declaringType, covariantDeclaringType, result);

        if (method.IsGenericMethod && covariantReturnMethod.IsGenericMethod)
        {
            var methodGenericArguments = method.GetGenericArguments();
            var covariantGenericArguments = covariantReturnMethod.GetGenericArguments();
            foreach (var (methodGenericArgument, covariantGenericArgument) in methodGenericArguments.Zip(covariantGenericArguments))
            {
                if (methodGenericArgument.IsGenericParameter)
                    result[methodGenericArgument] = covariantGenericArgument;
            }
        }

        return result;
    }

    /// <summary>
    /// Adds mappings for generic parameters declared on the base declaring type.
    /// </summary>
    /// <param name="declaringType">The type that declares the base or less-derived method.</param>
    /// <param name="covariantDeclaringType">The type that declares the covariant-return method.</param>
    /// <param name="result">The map receiving generic parameter substitutions.</param>
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

    /// <summary>
    /// Finds the base type or interface on <paramref name="type"/> that corresponds to <paramref name="declaringType"/>.
    /// </summary>
    /// <param name="type">The derived type to inspect.</param>
    /// <param name="declaringType">The declaring type whose constructed form should be located.</param>
    /// <returns>
    /// The matching constructed base type or interface, or <see langword="null"/> if no matching type is found.
    /// </returns>
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

    /// <summary>
    /// Enumerates a type, its base types, and the interfaces visible from each level.
    /// </summary>
    /// <param name="type">The type whose inheritance graph should be traversed.</param>
    /// <returns>The type itself, followed by base types and implemented interfaces.</returns>
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

    /// <summary>
    /// Replaces generic parameters inside a type with their mapped generic parameters or concrete generic arguments.
    /// </summary>
    /// <param name="type">The type whose generic parameters should be substituted.</param>
    /// <param name="genericParameterMap">The substitution map created from the compared methods.</param>
    /// <returns>The substituted type, or the original type when no substitution is required.</returns>
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

    /// <summary>
    /// Compares array and generic type shapes using custom comparers for generic arguments and type definitions.
    /// </summary>
    /// <param name="type">The first type to compare.</param>
    /// <param name="other">The second type to compare.</param>
    /// <param name="argumentComparer">The comparer used for generic arguments or array element types.</param>
    /// <param name="typeDefinitionComparer">The comparer used for generic type definitions.</param>
    /// <returns><see langword="true"/> if the two types have compatible generic or array shapes; otherwise, <see langword="false"/>.</returns>
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

        var genericArguments = type.GetGenericArguments();
        var otherGenericArguments = other.GetGenericArguments();

        if (genericArguments.Length != otherGenericArguments.Length)
            return false;

        var genericTypeDefinition = type.GetGenericTypeDefinition();
        var otherGenericTypeDefinition = other.GetGenericTypeDefinition();

        if (typeDefinitionComparer(genericTypeDefinition, otherGenericTypeDefinition) == false)
            return false;

        foreach (var (genericArgument, otherGenericArgument) in genericArguments.Zip(otherGenericArguments))
        {
            if (argumentComparer(genericArgument, otherGenericArgument) == false)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether the specified type is a covariant generic parameter.
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

    /// <summary>
    /// Determines whether two generic parameters represent the same generic slot.
    /// </summary>
    /// <param name="type">The first generic parameter to compare.</param>
    /// <param name="other">The second generic parameter to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both parameters have the same position and declaring method or type; otherwise,
    /// <see langword="false"/>.
    /// </returns>
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

    /// <summary>
    /// Determines whether a generic type definition is assignable from another type or generic type definition.
    /// </summary>
    /// <param name="type">The generic type definition that may be assignable from <paramref name="other"/>.</param>
    /// <param name="other">The type or generic type definition to test.</param>
    /// <returns><see langword="true"/> if <paramref name="type"/> is assignable from <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
    public static bool IsAssignableFromGenericTypeDefinition(this Type type, Type other)
    {
        if (type.IsGenericTypeDefinition == false)
            return false;

        var current = other;
        while (true)
        {
            if (type.IsAssignableFrom(current))
                return true;

            if (current.IsGenericTypeDefinition == false)
                return false;

            foreach (var implementedInterface in current.GetInterfaces())
            {
                if (implementedInterface.IsGenericType == false)
                    continue;

                if (type.IsAssignableFrom(implementedInterface.GetGenericTypeDefinition()))
                    return true;
            }

            current = current.BaseType;

            if (current is null)
                break;

            if (current.IsConstructedGenericType)
                current = current.GetGenericTypeDefinition();
        }

        return false;
    }

    /// <summary>
    /// Normalizes matching by-ref wrappers before comparing the underlying types.
    /// </summary>
    /// <param name="type">The first type, replaced with its element type when it is by-ref.</param>
    /// <param name="other">The second type, replaced with its element type when it is by-ref.</param>
    /// <returns>
    /// <see langword="true"/> if both types are either by-ref or not by-ref; otherwise, <see langword="false"/>.
    /// </returns>
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

    /// <summary>
    /// Determines whether one type can accept another type in covariant-return matching.
    /// </summary>
    /// <param name="type">The expected base or less-derived type.</param>
    /// <param name="other">The candidate derived or more-specific type.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="other"/> is assignable to <paramref name="type"/> under
    /// covariant-return rules; otherwise, <see langword="false"/>.
    /// </returns>
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

    /// <summary>
    /// Determines whether two types are equivalent for covariant-return signature matching.
    /// </summary>
    /// <param name="type">The first type to compare.</param>
    /// <param name="other">The second type to compare.</param>
    /// <returns>
    /// <see langword="true"/> if the two types are exactly equivalent after accounting for generic parameters and
    /// by-ref wrappers; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsCovariantReturnEquivalentTo(this Type type, Type other)
    {
        if (TryUnwrapByRef(ref type, ref other) == false)
            return false;

        return type == other
               || AreEquivalentGenericParameters(type, other)
               || AreEquivalentGenericTypes(type, other, IsCovariantReturnEquivalentTo, (a, b) => a == b);
    }
}
