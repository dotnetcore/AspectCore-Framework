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
        // Type-level candidates from the current compilation's syntax trees (fast path)
        var candidateTypes = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is TypeDeclarationSyntax tds && tds.AttributeLists.Count > 0,
                static (ctx, _) => GetCandidate(ctx))
            .Where(static x => x is not null)
            .Select(static (x, _) => x!);

        // Also discover candidates from referenced assemblies (multi-assembly support)
        var referencedAssemblyCandidates = context.CompilationProvider
            .SelectMany(static (compilation, _) => GetReferencedAssemblyCandidates(compilation));

        var allCandidates = candidateTypes.Collect()
            .Combine(referencedAssemblyCandidates.Collect())
            .Select(static (pair, _) => pair.Left.Concat(pair.Right).ToImmutableArray());

        context.RegisterSourceOutput(context.CompilationProvider.Combine(allCandidates), static (spc, input) =>
        {
            var (compilation, candidates) = input;
            Execute(spc, compilation, candidates);
        });
    }

    /// <summary>
    /// Discovers types decorated with [AspectCoreGenerateProxy] in referenced assemblies.
    /// This enables multi-assembly scenarios where the attribute is placed in a referenced library.
    /// </summary>
    private static ImmutableArray<INamedTypeSymbol> GetReferencedAssemblyCandidates(Compilation compilation)
    {
        var attrSymbol = compilation.GetTypeByMetadataName(GenerateProxyAttributeMetadataName);
        if (attrSymbol is null)
            return ImmutableArray<INamedTypeSymbol>.Empty;

        var results = new List<INamedTypeSymbol>();

        // Scan all referenced assemblies
        foreach (var referencedAssembly in compilation.References)
        {
            var assemblySymbol = compilation.GetAssemblyOrModuleSymbol(referencedAssembly) as IAssemblySymbol;
            if (assemblySymbol is null)
                continue;

            // Check if the assembly has the attribute at assembly level
            var hasAssemblyAttr = assemblySymbol.GetAttributes()
                .Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attrSymbol));

            if (hasAssemblyAttr)
            {
                // Assembly-level: discover all eligible types
                foreach (var type in EnumerateAssemblyTypes(assemblySymbol.GlobalNamespace))
                {
                    if (IsEligibleForAutoProxy(type))
                        results.Add(type);
                }
            }
            else
            {
                // Type-level: only discover types that explicitly carry the attribute
                foreach (var type in EnumerateAssemblyTypes(assemblySymbol.GlobalNamespace))
                {
                    if (type.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attrSymbol)))
                        results.Add(type);
                }
            }
        }

        return results.ToImmutableArray();
    }

    private static IEnumerable<INamedTypeSymbol> EnumerateAssemblyTypes(INamespaceSymbol ns)
    {
        foreach (var type in ns.GetTypeMembers())
            yield return type;
        foreach (var childNs in ns.GetNamespaceMembers())
        {
            foreach (var type in EnumerateAssemblyTypes(childNs))
                yield return type;
        }
    }

    private static bool IsEligibleForAutoProxy(INamedTypeSymbol type)
    {
        if (type.ContainingType is not null) return false; // skip nested
        if (type.IsStatic) return false;
        if (type.IsRefLikeType) return false; // skip ref structs (cannot be boxed/interfaced/class fields)
        if (type.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal)) return false;

        if (type.TypeKind == TypeKind.Class)
        {
            if (type.IsSealed && !type.IsAbstract) return false;
            // Must have at least one overridable member
            var isRecord = RecordTypeUtils.IsRecord(type);
            foreach (var member in type.GetMembers())
            {
                if (member is IMethodSymbol m && IsProxyableClassMethod(type, m, isRecord))
                    return true;
                if (member is IPropertySymbol p && IsProxyableClassProperty(type, p, isRecord))
                    return true;
            }
            return false;
        }

        if (type.TypeKind == TypeKind.Interface)
            return true;

        return false;
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
        // Supports:
        // - type-level [AspectCoreGenerateProxy]: explicit per-type proxy generation
        // - assembly-level [AspectCoreGenerateProxy]: auto-generate for all eligible types in the assembly
        // - class: generates class proxy (serviceType=implType=该类)
        // - interface: generates interface proxy (无 target / 带 target)

        var attrSymbol = compilation.GetTypeByMetadataName(GenerateProxyAttributeMetadataName);
        if (attrSymbol is null)
        {
            // 用户未引用包含 Attribute 的 runtime 包，直接不输出。
            return;
        }

        // Check for assembly-level attribute
        var hasAssemblyLevelAttr = compilation.Assembly.GetAttributes()
            .Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attrSymbol));

        // Collect all candidate types: explicit type-level + auto-discovered from assembly-level
        var allCandidates = new HashSet<INamedTypeSymbol>(NamedTypeSymbolEqualityComparer.Instance);
        foreach (var t in candidates)
            allCandidates.Add(t);

        if (hasAssemblyLevelAttr)
        {
            foreach (var t in GetAssemblyEligibleTypes(compilation, attrSymbol))
                allCandidates.Add(t);
        }

        var entries = new List<ProxyEntry>();
        foreach (var type in allCandidates.Distinct(NamedTypeSymbolEqualityComparer.Instance))
        {
            var attrData = type.GetAttributes().FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attrSymbol));
            if (attrData is null)
            {
                continue;
            }

            // Generic types are supported for class proxy (generic params forwarded).
            // Interface proxy for generic interfaces is also supported.

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

            // P0: 检查 ref struct 类型（ref struct 不能装箱、不能实现接口、不能作为类字段）
            if (type.IsRefLikeType)
            {
                context.ReportDiagnostic(GeneratorDiagnostics.RefStructNotSupported(type));
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

        var emittedEntries = new List<ProxyEntry>();
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
                emittedEntries.Add(entry);
            }
        }

        if (emittedEntries.Count > 0)
        {
            context.AddSource("AspectCoreSourceGeneratedProxyRegistry.g.cs", RegistryEmitter.EmitRegistry(emittedEntries));
        }
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

    /// <summary>
    /// Discovers all eligible types in the assembly for auto-proxy generation when
    /// [assembly: AspectCoreGenerateProxy] is used. Eligible types are public classes
    /// and interfaces that are not sealed (for classes), not nested, not abstract (for classes),
    /// and have at least one overridable member.
    /// </summary>
    private static IEnumerable<INamedTypeSymbol> GetAssemblyEligibleTypes(Compilation compilation, INamedTypeSymbol attrSymbol)
    {
        var globalNamespace = compilation.GlobalNamespace;
        foreach (var type in EnumerateAllTypes(globalNamespace))
        {
            // Skip types that already have explicit type-level attribute (they'll be handled separately)
            if (type.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attrSymbol)))
                continue;

            // Skip nested types
            if (type.ContainingType is not null)
                continue;

            // Skip types that are not public or internal
            if (type.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal))
                continue;

            // For classes: must not be sealed (unless abstract), and must have at least one overridable member
            if (type.TypeKind == TypeKind.Class)
            {
                if (type.IsSealed && !type.IsAbstract)
                    continue;
                if (type.IsStatic)
                    continue;
                // Check if there's at least one overridable method or property
                if (!HasAnyOverridableMember(type))
                    continue;
            }
            else if (type.TypeKind == TypeKind.Interface)
            {
                // Interfaces are always eligible
            }
            else
            {
                continue;
            }

            // Skip types with events (not supported)
            if (type.GetMembers().OfType<IEventSymbol>().Any())
                continue;

            yield return type;
        }
    }

    private static IEnumerable<INamedTypeSymbol> EnumerateAllTypes(INamespaceSymbol ns)
    {
        foreach (var type in ns.GetTypeMembers())
        {
            yield return type;
        }
        foreach (var childNs in ns.GetNamespaceMembers())
        {
            foreach (var type in EnumerateAllTypes(childNs))
            {
                yield return type;
            }
        }
    }

    private static bool HasAnyOverridableMember(INamedTypeSymbol type)
    {
        var isRecord = RecordTypeUtils.IsRecord(type);
        foreach (var member in type.GetMembers())
        {
            if (member is IMethodSymbol m && IsProxyableClassMethod(type, m, isRecord))
            {
                return true;
            }
            if (member is IPropertySymbol p && IsProxyableClassProperty(type, p, isRecord))
            {
                return true;
            }
        }
        return false;
    }

    private static bool IsProxyableClassMethod(INamedTypeSymbol type, IMethodSymbol method, bool isRecord)
    {
        return method.MethodKind == MethodKind.Ordinary
               && !method.IsStatic
               && method.IsVirtual
               && !method.IsSealed
               && method.DeclaredAccessibility is Accessibility.Public or Accessibility.Protected or Accessibility.ProtectedOrInternal
               && !RecordTypeUtils.IsRecordSynthesizedMember(type, method, isRecord);
    }

    private static bool IsProxyableClassProperty(INamedTypeSymbol type, IPropertySymbol property, bool isRecord)
    {
        return !property.IsStatic
               && property.IsVirtual
               && !property.IsSealed
               && property.DeclaredAccessibility is Accessibility.Public or Accessibility.Protected or Accessibility.ProtectedOrInternal
               && !RecordTypeUtils.IsRecordSynthesizedMember(type, property, isRecord);
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
