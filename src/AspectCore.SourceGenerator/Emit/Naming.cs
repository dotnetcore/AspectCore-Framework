using System.Text;
using Microsoft.CodeAnalysis;

namespace AspectCore.SourceGenerator;

internal static class Naming
{
    public const string GeneratedProxyNamespace = "AspectCore.SourceGenerated.Proxies";

    public static string GetProxyTypeName(INamedTypeSymbol serviceType, INamedTypeSymbol? implType, ProxyKind kind)
    {
        // Deterministic + collision-resistant enough for this node.
        static string Sanitize(string s)
        {
            var sb = new StringBuilder(s.Length);
            foreach (var ch in s)
            {
                sb.Append(char.IsLetterOrDigit(ch) ? ch : '_');
            }
            return sb.ToString();
        }

        var serviceId = Sanitize(serviceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        var implId = implType is null ? "NoTarget" : Sanitize(implType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        return $"{serviceId}__{implId}__{kind}Proxy";
    }
}

