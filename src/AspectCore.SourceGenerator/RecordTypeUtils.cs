using System.Linq;
using Microsoft.CodeAnalysis;

namespace AspectCore.SourceGenerator;

/// <summary>
/// Shared utilities for detecting C# 9 record types and their compiler-synthesized
/// members. Used by both <see cref="AspectCoreProxyGenerator"/> and
/// <see cref="ProxyEmitter"/> to avoid logic drift between the two code paths.
/// </summary>
internal static class RecordTypeUtils
{
    /// <summary>
    /// Determines whether the given type is a C# 9 record type by checking for the
    /// compiler-synthesized copy method (<c>&lt;Clone&gt;$</c> or <c>&lt;&gt;Copy</c>),
    /// or by checking whether any base type is a record (a type derived from a record
    /// is itself a record).
    /// </summary>
    internal static bool IsRecord(INamedTypeSymbol type)
    {
        for (var current = type; current is not null && current.SpecialType != SpecialType.System_Object; current = current.BaseType)
        {
            if (current.GetMembers().OfType<IMethodSymbol>().Any(IsRecordCopyMethod))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the given method is the record copy method
    /// (<c>&lt;Clone&gt;$</c> or <c>&lt;&gt;Copy</c>) synthesized by the compiler.
    /// </summary>
    /// <remarks>
    /// The copy method is <c>virtual</c> in a base record but <c>override sealed</c> in
    /// a derived record. We therefore do NOT require <c>!IsSealed</c> here; otherwise
    /// derived records would not be detected as records at all.
    /// </remarks>
    internal static bool IsRecordCopyMethod(IMethodSymbol method)
        => method.MethodKind == MethodKind.Ordinary
           && method.IsVirtual
           && method.Parameters.Length == 0
           && SymbolEqualityComparer.Default.Equals(method.ReturnType, method.ContainingType)
           && (method.Name == "<Clone>$" || method.Name == "<>Copy");

    /// <summary>
    /// Determines whether the given symbol is a compiler-synthesized record member
    /// (implicitly declared, compiler-generated, or the copy method) that should be
    /// excluded from proxy generation.
    /// </summary>
    /// <remarks>
    /// This overload recomputes <see cref="IsRecord"/> on every call. For hot paths
    /// where this is called for many members of the same type, use the overload that
    /// accepts a pre-computed <paramref name="isRecord"/> flag to avoid O(N²) cost.
    /// </remarks>
    internal static bool IsRecordSynthesizedMember(INamedTypeSymbol type, ISymbol symbol)
        => IsRecordSynthesizedMember(type, symbol, IsRecord(type));

    /// <summary>
    /// Determines whether the given symbol is a compiler-synthesized record member,
    /// using a pre-computed <paramref name="isRecord"/> flag to avoid re-scanning all
    /// members of the type.
    /// </summary>
    internal static bool IsRecordSynthesizedMember(INamedTypeSymbol type, ISymbol symbol, bool isRecord)
    {
        if (!isRecord)
        {
            return false;
        }

        if (symbol.IsImplicitlyDeclared)
        {
            return true;
        }

        if (symbol.GetAttributes().Any(a =>
                a.AttributeClass?.ToDisplayString() == "System.Runtime.CompilerServices.CompilerGeneratedAttribute"))
        {
            return true;
        }

        return symbol is IMethodSymbol method && IsRecordCopyMethod(method);
    }
}
