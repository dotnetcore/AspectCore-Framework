using Microsoft.CodeAnalysis;

namespace AspectCore.SourceGenerator;

internal static class TypeNameExtensions
{
    public static string ToGlobalName(this ITypeSymbol symbol)
        => symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    public static string ToGlobalName(this INamedTypeSymbol symbol)
        => symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
}

