using System.Linq;
using Microsoft.CodeAnalysis;

namespace AspectCore.SourceGenerator;

internal static class GeneratorDiagnostics
{
    private static readonly DiagnosticDescriptor UnsupportedGenericTypeDescriptor = new(
        id: "ACSG001",
        title: "AspectCore SourceGenerator 暂不支持开放泛型类型",
        messageFormat: "类型 '{0}' 为开放泛型，当前版本的 Source Generator 暂不支持生成代理。",
        category: "AspectCore.SourceGenerator",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor UnsupportedNestedTypeDescriptor = new(
        id: "ACSG002",
        title: "AspectCore SourceGenerator 暂不支持嵌套类型",
        messageFormat: "类型 '{0}' 为嵌套类型，当前版本的 Source Generator 暂不支持生成代理。",
        category: "AspectCore.SourceGenerator",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor UnsupportedEventDescriptor = new(
        id: "ACSG003",
        title: "AspectCore SourceGenerator 暂不支持事件成员",
        messageFormat: "类型 '{0}' 包含事件成员 '{1}'，当前版本的 Source Generator 暂不支持生成代理。",
        category: "AspectCore.SourceGenerator",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor UnsupportedGenericMethodDescriptor = new(
        id: "ACSG004",
        title: "AspectCore SourceGenerator 暂不支持开放泛型方法",
        messageFormat: "类型 '{0}' 包含开放泛型方法 '{1}'，当前版本的 Source Generator 暂不支持生成代理。",
        category: "AspectCore.SourceGenerator",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor SealedTypeDescriptor = new(
        id: "ACSG005",
        title: "无法为 sealed 类型生成代理",
        messageFormat: "无法为 sealed 类型 '{0}' 生成代理。请移除 sealed 修饰符或使用接口代理。",
        category: "AspectCore.SourceGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor TypeNotAccessibleDescriptor = new(
        id: "ACSG006",
        title: "类型对 Source Generator 不可见",
        messageFormat: "类型 '{0}' 对 Source Generator 不可见。请确保类型具有 public 或 internal 可访问性。",
        category: "AspectCore.SourceGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor NoAccessibleConstructorDescriptor = new(
        id: "ACSG007",
        title: "类型没有可访问的构造函数",
        messageFormat: "类型 '{0}' 没有可访问的构造函数。类代理要求目标类型具有 public 或 protected 构造函数。",
        category: "AspectCore.SourceGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor RefStructNotSupportedDescriptor = new(
        id: "ACSG008",
        title: "无法为 ref struct 类型生成代理",
        messageFormat: "无法为 ref struct 类型 '{0}' 生成代理。ref struct（如 Span<T>、ReadOnlySpan<T>）不能装箱、不能实现接口、不能作为类字段，因此无法进行 AOP 代理。",
        category: "AspectCore.SourceGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor UnsupportedByRefLikeParamsDescriptor = new(
        id: "ACSG009",
        title: "AspectCore SourceGenerator 暂不支持 byref-like params 参数",
        messageFormat: "成员 '{0}' 包含 byref-like params 参数 '{1}'，当前版本的 Source Generator 暂不支持生成代理。",
        category: "AspectCore.SourceGenerator",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor OpenGenericMethodNativeAotFallbackDescriptor = new(
        id: "ACSG0101",
        title: "Open generic method falls back to reflection for NativeAOT",
        messageFormat: "Method '{0}.{1}' is an open generic. The NativeAOT delegate falls back to reflection for unclosed type parameters. Add [AspectCoreGenericHint] to specify concrete type arguments for full NativeAOT safety.",
        category: "AspectCore.SourceGenerator",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static Diagnostic UnsupportedGenericType(INamedTypeSymbol symbol)
        => Diagnostic.Create(UnsupportedGenericTypeDescriptor, symbol.Locations.FirstOrDefault(), symbol.ToDisplayString());

    public static Diagnostic UnsupportedNestedType(INamedTypeSymbol symbol)
        => Diagnostic.Create(UnsupportedNestedTypeDescriptor, symbol.Locations.FirstOrDefault(), symbol.ToDisplayString());

    public static Diagnostic UnsupportedEvent(INamedTypeSymbol type, IEventSymbol ev)
        => Diagnostic.Create(UnsupportedEventDescriptor, ev.Locations.FirstOrDefault() ?? type.Locations.FirstOrDefault(), type.ToDisplayString(), ev.Name);

    public static Diagnostic UnsupportedGenericMethod(INamedTypeSymbol type, IMethodSymbol method)
        => Diagnostic.Create(UnsupportedGenericMethodDescriptor, method.Locations.FirstOrDefault() ?? type.Locations.FirstOrDefault(), type.ToDisplayString(), method.ToDisplayString());

    public static Diagnostic SealedType(INamedTypeSymbol symbol)
        => Diagnostic.Create(SealedTypeDescriptor, symbol.Locations.FirstOrDefault(), symbol.ToDisplayString());

    public static Diagnostic TypeNotAccessible(INamedTypeSymbol symbol)
        => Diagnostic.Create(TypeNotAccessibleDescriptor, symbol.Locations.FirstOrDefault(), symbol.ToDisplayString());

    public static Diagnostic NoAccessibleConstructor(INamedTypeSymbol symbol)
        => Diagnostic.Create(NoAccessibleConstructorDescriptor, symbol.Locations.FirstOrDefault(), symbol.ToDisplayString());

    public static Diagnostic RefStructNotSupported(INamedTypeSymbol symbol)
        => Diagnostic.Create(RefStructNotSupportedDescriptor, symbol.Locations.FirstOrDefault(), symbol.ToDisplayString());

    public static Diagnostic UnsupportedByRefLikeParams(IMethodSymbol method, IParameterSymbol parameter)
        => Diagnostic.Create(
            UnsupportedByRefLikeParamsDescriptor,
            parameter.Locations.FirstOrDefault() ?? method.Locations.FirstOrDefault(),
            method.ToDisplayString(),
            parameter.Name);

    public static Diagnostic OpenGenericMethodNativeAotFallback(INamedTypeSymbol type, IMethodSymbol method)
        => Diagnostic.Create(
            OpenGenericMethodNativeAotFallbackDescriptor,
            method.Locations.FirstOrDefault() ?? type.Locations.FirstOrDefault(),
            type.ToDisplayString(),
            method.ToDisplayString());
}

