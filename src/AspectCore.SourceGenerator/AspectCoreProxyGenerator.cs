using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AspectCore.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public sealed class AspectCoreProxyGenerator : IIncrementalGenerator
{
    internal const string GenerateProxyAttributeMetadataName = "AspectCore.DynamicProxy.AspectCoreGenerateProxyAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var candidateTypes = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is TypeDeclarationSyntax tds && tds.AttributeLists.Count > 0,
                static (ctx, _) => GetCandidate(ctx))
            .Where(static x => x is not null)
            .Select(static (x, _) => x!);

        context.RegisterSourceOutput(context.CompilationProvider.Combine(candidateTypes.Collect()), static (spc, input) =>
        {
            var (compilation, candidates) = input;
            Execute(spc, compilation, candidates);
        });
    }

    private static INamedTypeSymbol? GetCandidate(GeneratorSyntaxContext ctx)
    {
        if (ctx.Node is not TypeDeclarationSyntax tds)
        {
            return null;
        }

        var symbol = ctx.SemanticModel.GetDeclaredSymbol(tds) as INamedTypeSymbol;
        if (symbol is null)
        {
            return null;
        }

        foreach (var attr in symbol.GetAttributes())
        {
            var attrClass = attr.AttributeClass;
            if (attrClass is null) continue;
            if (attrClass.ToDisplayString() == GenerateProxyAttributeMetadataName)
            {
                return symbol;
            }
        }

        return null;
    }

    private static void Execute(SourceProductionContext context, Compilation compilation, ImmutableArray<INamedTypeSymbol> candidates)
    {
        // NOTE: 本节点先实现 Attribute 触发：只处理 type-level [AspectCoreGenerateProxy]。
        // - class: 默认生成 class proxy (serviceType=implType=该类)
        // - interface: 默认生成 interface proxy（生成两种 ctor：无 target / 带 target）
        // assembly-level mapping / 带参数 mapping 在后续节点补齐。

        var attrSymbol = compilation.GetTypeByMetadataName(GenerateProxyAttributeMetadataName);
        if (attrSymbol is null)
        {
            // 用户未引用包含 Attribute 的 runtime 包，直接不输出。
            return;
        }

        var entries = new List<ProxyEntry>();
        foreach (var type in candidates.Distinct(NamedTypeSymbolEqualityComparer.Instance))
        {
            var attrData = type.GetAttributes().FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attrSymbol));
            if (attrData is null)
            {
                continue;
            }

            if (type.IsGenericType)
            {
                context.ReportDiagnostic(GeneratorDiagnostics.UnsupportedGenericType(type));
                continue;
            }

            if (type.ContainingType is not null)
            {
                context.ReportDiagnostic(GeneratorDiagnostics.UnsupportedNestedType(type));
                continue;
            }

            // P1-1: 检查 sealed 类型（对于 class proxy）
            if (type.TypeKind == TypeKind.Class && type.IsSealed && !type.IsAbstract)
            {
                context.ReportDiagnostic(GeneratorDiagnostics.SealedType(type));
                continue;
            }

            // P1-2: 检查类型可见性
            if (!IsTypeAccessible(type, compilation))
            {
                context.ReportDiagnostic(GeneratorDiagnostics.TypeNotAccessible(type));
                continue;
            }

            // 从 attribute 中读取实现类型
            INamedTypeSymbol? implementationType = null;
            foreach (var namedArg in attrData.NamedArguments)
            {
                if (namedArg.Key == "ImplementationType" && namedArg.Value.Value is INamedTypeSymbol implType)
                {
                    implementationType = implType;
                    break;
                }
            }

            // 从构造函数参数中读取实现类型
            if (implementationType is null && attrData.ConstructorArguments.Length > 0)
            {
                // 构造函数参数顺序：serviceType, implementationType, kind
                // 或者：implementationType (单参数构造函数)
                foreach (var arg in attrData.ConstructorArguments)
                {
                    if (arg.Value is INamedTypeSymbol implType)
                    {
                        // 检查这个类型是否是 implementationType（不是 serviceType）
                        // 通过检查 attribute 构造函数的参数顺序来确定
                        if (attrData.ConstructorArguments.Length == 1)
                        {
                            // 单参数构造函数：implementationType
                            implementationType = implType;
                        }
                        else if (attrData.ConstructorArguments.Length >= 2)
                        {
                            // 多参数构造函数：第二个参数是 implementationType
                            var secondArg = attrData.ConstructorArguments[1];
                            if (secondArg.Value is INamedTypeSymbol implType2)
                            {
                                implementationType = implType2;
                            }
                        }
                        break;
                    }
                }
            }

            // 验证实现类型的可见性
            if (implementationType is not null && !IsTypeAccessible(implementationType, compilation))
            {
                context.ReportDiagnostic(GeneratorDiagnostics.TypeNotAccessible(implementationType));
                continue;
            }

            switch (type.TypeKind)
            {
                case TypeKind.Interface:
                    entries.Add(ProxyEntry.CreateInterface(serviceType: type, implementationType));
                    break;
                case TypeKind.Class:
                    // P1-3: 检查构造函数可访问性
                    if (!HasAccessibleConstructor(type))
                    {
                        context.ReportDiagnostic(GeneratorDiagnostics.NoAccessibleConstructor(type));
                        continue;
                    }
                    entries.Add(ProxyEntry.CreateClass(serviceType: type, implementationType: type));
                    break;
            }
        }

        if (entries.Count == 0)
        {
            return;
        }

        foreach (var entry in entries)
        {
            var src = entry.Kind switch
            {
                ProxyKind.Interface => ProxyEmitter.EmitInterfaceProxy(compilation, entry, context),
                ProxyKind.Class => ProxyEmitter.EmitClassProxy(compilation, entry, context),
                _ => null
            };

            if (src is not null)
            {
                context.AddSource($"{entry.ProxyTypeName}.g.cs", src);
            }
        }

        context.AddSource("AspectCoreSourceGeneratedProxyRegistry.g.cs", RegistryEmitter.EmitRegistry(entries));
    }

    /// <summary>
    /// 检查类型是否对生成器可见（考虑 internal 和 InternalsVisibleTo）
    /// </summary>
    private static bool IsTypeAccessible(INamedTypeSymbol type, Compilation compilation)
    {
        // Public 类型总是可见
        if (type.DeclaredAccessibility == Accessibility.Public)
        {
            // 对于嵌套类型，需要检查所有包含类型的可见性
            if (type.ContainingType is not null)
            {
                return IsTypeAccessible(type.ContainingType, compilation);
            }
            return true;
        }

        // Internal 类型：Source Generator 生成的代码在同一程序集中，因此可见
        if (type.DeclaredAccessibility == Accessibility.Internal)
        {
            if (type.ContainingType is not null)
            {
                return IsTypeAccessible(type.ContainingType, compilation);
            }
            return true;
        }

        // ProtectedOrInternal：同一程序集可见（生成的代码在同一程序集）
        if (type.DeclaredAccessibility == Accessibility.ProtectedOrInternal)
        {
            if (type.ContainingType is not null)
            {
                return IsTypeAccessible(type.ContainingType, compilation);
            }
            return true;
        }

        // Protected、ProtectedAndInternal、Private：生成的代理类无法访问
        // Protected 只能在包含类或派生类中访问
        // Private 只能在包含类中访问
        return false;
    }

    /// <summary>
    /// 检查类型是否有可访问的构造函数（用于 class proxy）
    /// </summary>
    private static bool HasAccessibleConstructor(INamedTypeSymbol type)
    {
        if (type.InstanceConstructors.Length == 0)
        {
            return false;
        }

        // 检查是否有 public 或 protected 构造函数
        foreach (var ctor in type.InstanceConstructors)
        {
            if (ctor.DeclaredAccessibility is Accessibility.Public or Accessibility.Protected or Accessibility.ProtectedOrInternal)
            {
                return true;
            }
        }

        return false;
    }
}

internal sealed class NamedTypeSymbolEqualityComparer : IEqualityComparer<INamedTypeSymbol>
{
    public static readonly NamedTypeSymbolEqualityComparer Instance = new();

    public bool Equals(INamedTypeSymbol? x, INamedTypeSymbol? y)
        => SymbolEqualityComparer.Default.Equals(x, y);

    public int GetHashCode(INamedTypeSymbol obj)
        => SymbolEqualityComparer.Default.GetHashCode(obj);
}

internal enum ProxyKind
{
    Interface = 0,
    Class = 1,
}

internal sealed class ProxyEntry
{
    public ProxyEntry(INamedTypeSymbol serviceType, INamedTypeSymbol? implementationType, ProxyKind kind, string proxyTypeName, string proxyNamespace)
    {
        ServiceType = serviceType;
        ImplementationType = implementationType;
        Kind = kind;
        ProxyTypeName = proxyTypeName;
        ProxyNamespace = proxyNamespace;
    }

    public INamedTypeSymbol ServiceType { get; }
    public INamedTypeSymbol? ImplementationType { get; }
    public ProxyKind Kind { get; }
    public string ProxyTypeName { get; }
    public string ProxyNamespace { get; }

    public static ProxyEntry CreateInterface(INamedTypeSymbol serviceType, INamedTypeSymbol? implementationType)
        => new(serviceType, implementationType, kind: ProxyKind.Interface,
            proxyTypeName: Naming.GetProxyTypeName(serviceType, implementationType, ProxyKind.Interface),
            proxyNamespace: Naming.GeneratedProxyNamespace);

    public static ProxyEntry CreateClass(INamedTypeSymbol serviceType, INamedTypeSymbol implementationType)
        => new(serviceType, implementationType, ProxyKind.Class,
            Naming.GetProxyTypeName(serviceType, implementationType, ProxyKind.Class),
            Naming.GeneratedProxyNamespace);
}
