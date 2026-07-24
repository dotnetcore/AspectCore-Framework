using System.Linq;
using Microsoft.CodeAnalysis;

namespace AspectCore.SourceGenerator;

internal enum NativeAotSignatureDiagnosticKind
{
    None,
    ByRefLikeParamsParameter,
    ByRefLikeParameter,
    ByRefLikeReturn,
}

internal readonly struct NativeAotSignatureDiagnostic
{
    public NativeAotSignatureDiagnostic(NativeAotSignatureDiagnosticKind kind, IMethodSymbol method, IParameterSymbol? parameter)
    {
        Kind = kind;
        Method = method;
        Parameter = parameter;
    }

    public NativeAotSignatureDiagnosticKind Kind { get; }

    public IMethodSymbol Method { get; }

    public IParameterSymbol? Parameter { get; }

    public bool HasDiagnostic => Kind != NativeAotSignatureDiagnosticKind.None;
}

internal static class NativeAotSignatureDiagnosticRules
{
    public static NativeAotSignatureDiagnostic Analyze(IMethodSymbol method)
    {
        if (!method.ReturnsVoid && IsByRefLikeType(method.ReturnType))
        {
            return new NativeAotSignatureDiagnostic(
                NativeAotSignatureDiagnosticKind.ByRefLikeReturn,
                method,
                parameter: null);
        }

        var parameter = method.Parameters.FirstOrDefault(p => IsByRefLikeType(p.Type));
        if (parameter is null)
        {
            return new NativeAotSignatureDiagnostic(
                NativeAotSignatureDiagnosticKind.None,
                method,
                parameter: null);
        }

        return new NativeAotSignatureDiagnostic(
            parameter.IsParams
                ? NativeAotSignatureDiagnosticKind.ByRefLikeParamsParameter
                : NativeAotSignatureDiagnosticKind.ByRefLikeParameter,
            method,
            parameter);
    }

    private static bool IsByRefLikeType(ITypeSymbol type)
    {
        return type is INamedTypeSymbol { IsRefLikeType: true };
    }
}
