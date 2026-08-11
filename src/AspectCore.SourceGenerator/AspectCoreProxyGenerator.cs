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
            .Select(static (x, _) => new Candidate(x!, isExplicit: true));

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
    /// Discovers proxy candidates in referenced assemblies. This enables multi-assembly
    /// scenarios where the attribute is placed in a referenced library:
    /// - assembly-level attribute there → auto-discover all eligible types (Explicit = false)
    /// - type-level attribute on individual types → explicit candidates (Explicit = true)
    /// </summary>
    private static ImmutableArray<Candidate> GetReferencedAssemblyCandidates(Compilation compilation)
    {
        var attrSymbol = compilation.GetTypeByMetadataName(GenerateProxyAttributeMetadataName);
        if (attrSymbol is null)
            return ImmutableArray<Candidate>.Empty;

        var results = new List<Candidate>();

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
                // Assembly-level: auto-discover all eligible types
                foreach (var type in EnumerateTypes(assemblySymbol.GlobalNamespace))
                {
                    if (IsEligibleForAutoProxy(type, attrSymbol))
                        results.Add(new Candidate(type, isExplicit: false));
                }
            }
            else
            {
                // Type-level: only discover types that explicitly carry the attribute
                foreach (var type in EnumerateTypes(assemblySymbol.GlobalNamespace))
                {
                    if (type.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attrSymbol)))
                        results.Add(new Candidate(type, isExplicit: true));
                }
            }
        }

        return results.ToImmutableArray();
    }

    private static IEnumerable<INamedTypeSymbol> EnumerateTypes(INamespaceSymbol ns)
    {
        foreach (var type in ns.GetTypeMembers())
            yield return type;
        foreach (var childNs in ns.GetNamespaceMembers())
        {
            foreach (var type in EnumerateTypes(childNs))
                yield return type;
        }
    }

    /// <summary>
    /// Determines whether <paramref name="type"/> is eligible for assembly-level auto-proxy
    /// generation when its assembly declares <c>[assembly: AspectCoreGenerateProxy]</c>.
    /// Types that explicitly carry the type-level attribute are excluded here — they flow
    /// through the explicit path and must keep their attribute metadata.
    /// </summary>
    private static bool IsEligibleForAutoProxy(INamedTypeSymbol type, INamedTypeSymbol attrSymbol)
    {
        // Skip types that already have explicit type-level attribute (handled separately)
        if (type.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attrSymbol)))
            return false;

        // Skip nested types
        if (type.ContainingType is not null)
            return false;

        // Skip static types (no instance members to intercept)
        if (type.IsStatic)
            return false;

        // Skip ref structs (cannot be boxed / used as interface impl / class field)
        if (type.IsRefLikeType)
            return false;

        // The generated proxy class is always public (ProxyEmitter hardcodes it), so auto-proxying
        // is only possible for public source types. Internal types — even within the current
        // compilation — would produce CS0060 (inconsistent accessibility): a public proxy cannot
        // derive from or implement a less-accessible type. Referenced assemblies additionally only
        // expose their public types to the consumer's generated code. The explicit path shares this
        // limitation.
        if (type.DeclaredAccessibility != Accessibility.Public)
            return false;

        // Skip types with events (event proxying is not supported by either engine)
        if (type.GetMembers().OfType<IEventSymbol>().Any())
            return false;

        if (type.TypeKind == TypeKind.Class)
        {
            if (type.IsSealed && !type.IsAbstract)
                return false;
            // Must have at least one overridable member
            return HasAnyOverridableMember(type);
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

    private static void Execute(SourceProductionContext context, Compilation compilation, ImmutableArray<Candidate> candidates)
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

        // Explicit candidates carry the type-level attribute and keep their attribute metadata
        // (e.g. the declared implementation type). Auto-discovered candidates from assembly-level
        // auto-discovery carry none, so they are created with default proxy semantics below.
        var explicitTypes = candidates
            .Where(c => c.Explicit)
            .Select(c => c.Type)
            .Distinct(NamedTypeSymbolEqualityComparer.Instance)
            .ToList();

        var autoDiscovered = candidates
            .Where(c => !c.Explicit)
            .Select(c => c.Type)
            .ToList();

        // Auto-discover eligible types in the current assembly when it declares the assembly-level attribute.
        // Note: iterate Assembly.GlobalNamespace (source-declared types only) — Compilation.GlobalNamespace
        // also surfaces metadata types from referenced assemblies.
        if (hasAssemblyLevelAttr)
        {
            foreach (var type in EnumerateTypes(compilation.Assembly.GlobalNamespace))
            {
                if (IsEligibleForAutoProxy(type, attrSymbol))
                    autoDiscovered.Add(type);
            }
        }

        var entries = new List<ProxyEntry>();
        foreach (var type in explicitTypes)
        {
            var attrData = type.GetAttributes().FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attrSymbol));
            if (attrData is null)
            {
                // Defensive: explicit candidates always carry the attribute; keep the guard so
                // the explicit path never treats an auto-discovered type as explicit.
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

        CollectAutoDiscoveredEntries(context, entries, autoDiscovered);

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
    /// Creates proxy entries for types discovered via assembly-level auto-discovery.
    /// These types carry no type-level attribute, so entries are built with default
    /// semantics: classes become class proxies (service = implementation = the type),
    /// interfaces become no-target stub proxies.
    /// </summary>
    private static void CollectAutoDiscoveredEntries(
        SourceProductionContext context, List<ProxyEntry> entries, IEnumerable<INamedTypeSymbol> types)
    {
        foreach (var type in types.Distinct(NamedTypeSymbolEqualityComparer.Instance))
        {
            switch (type.TypeKind)
            {
                case TypeKind.Interface:
                    // No implementation type is known for auto-discovered interfaces;
                    // emit a no-target stub proxy (members return default).
                    entries.Add(ProxyEntry.CreateInterface(serviceType: type, implementationType: null));
                    break;

                case TypeKind.Class:
                    if (!HasAccessibleConstructor(type))
                    {
                        context.ReportDiagnostic(GeneratorDiagnostics.NoAccessibleConstructor(type));
                        continue;
                    }
                    entries.Add(ProxyEntry.CreateClass(serviceType: type, implementationType: type));
                    break;
            }
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

/// <summary>
/// A proxy candidate discovered by the generator. <see cref="Explicit"/> distinguishes
/// types carrying the type-level <c>[AspectCoreGenerateProxy]</c> attribute from types
/// auto-discovered via the assembly-level attribute. Auto-discovered types carry no
/// attribute metadata, so their proxy entries are built with default semantics.
/// </summary>
internal readonly struct Candidate
{
    public Candidate(INamedTypeSymbol type, bool isExplicit)
    {
        Type = type;
        Explicit = isExplicit;
    }

    public INamedTypeSymbol Type { get; }
    public bool Explicit { get; }
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
