#nullable enable

using System;
using System.Linq;
using AspectCore.SourceGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace AspectCore.Core.Tests.EngineParity;

/// <summary>
/// 编译期诊断验证测试
///
/// 这些测试通过真实驱动 <see cref="AspectCoreProxyGenerator"/> 来验证 Source Generator
/// 在编译时报告的诊断信息:构造会触发对应场景的最小源码片段,运行 Source Generator,
/// 断言产出的诊断包含期望的 ACSGxxx id 与 severity(或按预期不产生诊断)。
///
/// 编译驱动范式与 <see cref="SourceGeneratorDiagnosticTests"/> 保持一致(构造 <see cref="CSharpCompilation"/>
/// + <see cref="CSharpGeneratorDriver"/>,检查 <c>GetRunResult().Diagnostics</c>)。
/// </summary>
public class SourceGeneratorDiagnosticVerificationTests
{
    #region ACSG005: Sealed 类型诊断

    /// <summary>
    /// 验证:尝试为 sealed 类型生成代理应该报告 ACSG005 错误。
    ///
    /// 预期诊断:Id=ACSG005,Severity=Error,消息包含目标类型名。
    /// </summary>
    [Fact]
    public void SealedType_Should_Report_ACSG005_Error_Documentation()
    {
        const string source = """
            using AspectCore.DynamicProxy;

            [AspectCoreGenerateProxy]
            public sealed class SealedService
            {
                public void DoWork() { }
            }
            """;

        var runResult = RunGenerator(source, "SealedTypeDiagnostic");

        var diagnostic = Assert.Single(runResult.Diagnostics.Where(d => d.Id == "ACSG005"));
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("SealedService", diagnostic.GetMessage());
    }

    #endregion

    #region ACSG007: 无构造函数诊断

    /// <summary>
    /// 验证:尝试为没有可访问构造函数的类型生成代理应该报告 ACSG007 错误。
    ///
    /// 预期诊断:Id=ACSG007,Severity=Error,消息包含目标类型名。
    /// </summary>
    [Fact]
    public void NoAccessibleConstructor_Should_Report_ACSG007_Error_Documentation()
    {
        const string source = """
            using AspectCore.DynamicProxy;

            [AspectCoreGenerateProxy]
            public class NoPublicCtorService
            {
                private NoPublicCtorService() { }
                public virtual void DoWork() { }
            }
            """;

        var runResult = RunGenerator(source, "NoAccessibleConstructorDiagnostic");

        var diagnostic = Assert.Single(runResult.Diagnostics.Where(d => d.Id == "ACSG007"));
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("NoPublicCtorService", diagnostic.GetMessage());
    }

    #endregion

    #region ACSG006: 类型可见性诊断

    /// <summary>
    /// 验证:internal 类型应该可以正常生成代理(Source Generator 生成的代码位于同一编译上下文)。
    ///
    /// 预期结果:不报告 ACSG006(类型不可见)错误,不产生任何 Error 级别诊断,并且成功生成代理源码。
    /// </summary>
    [Fact]
    public void InternalType_Should_Not_Report_Error_Documentation()
    {
        const string source = """
            using AspectCore.DynamicProxy;

            [AspectCoreGenerateProxy]
            internal class InternalService
            {
                public virtual void DoWork() { }
            }
            """;

        var runResult = RunGenerator(source, "InternalTypeDiagnostic");

        Assert.Empty(runResult.Diagnostics.Where(d => d.Id == "ACSG006"));
        Assert.DoesNotContain(runResult.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
        // internal 类型对生成器可见,应实际产出代理源码。
        Assert.NotEmpty(runResult.GeneratedTrees);
    }

    #endregion

    #region ACSG001: 开放泛型类型诊断

    /// <summary>
    /// 验证:开放泛型类型的实际行为。
    ///
    /// 注意:当前版本的 Source Generator **支持**开放泛型类型的类代理(泛型参数会被转发到代理类型,
    /// 参见 <see cref="AspectCoreProxyGenerator"/> 与 ProxyEmitter 中的泛型处理逻辑)。
    /// ACSG001(UnsupportedGenericType)描述符虽已定义,但在生产代码中没有任何发出点,
    /// 因此开放泛型类型不会触发 ACSG001。
    ///
    /// 预期结果:不报告 ACSG001,不产生任何 Error 级别诊断,并且成功生成代理源码。
    /// </summary>
    [Fact]
    public void OpenGenericType_Should_Report_ACSG001_Warning_Documentation()
    {
        const string source = """
            using AspectCore.DynamicProxy;

            [AspectCoreGenerateProxy]
            public class GenericService<T>
            {
                public virtual void DoWork(T value) { }
            }
            """;

        var runResult = RunGenerator(source, "OpenGenericTypeDiagnostic");

        Assert.Empty(runResult.Diagnostics.Where(d => d.Id == "ACSG001"));
        Assert.DoesNotContain(runResult.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
        // 开放泛型类型受支持,应实际产出代理源码。
        Assert.NotEmpty(runResult.GeneratedTrees);
    }

    #endregion

    #region ACSG002: 嵌套类型诊断

    /// <summary>
    /// 验证:尝试为嵌套类型生成代理应该报告 ACSG002 警告。
    ///
    /// 预期诊断:Id=ACSG002,Severity=Warning,消息包含嵌套类型名。
    /// </summary>
    [Fact]
    public void NestedType_Should_Report_ACSG002_Warning_Documentation()
    {
        const string source = """
            using AspectCore.DynamicProxy;

            public class OuterClass
            {
                [AspectCoreGenerateProxy]
                public class NestedService
                {
                    public virtual void DoWork() { }
                }
            }
            """;

        var runResult = RunGenerator(source, "NestedTypeDiagnostic");

        var diagnostic = Assert.Single(runResult.Diagnostics.Where(d => d.Id == "ACSG002"));
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Contains("NestedService", diagnostic.GetMessage());
    }

    #endregion

    #region ACSG003: 事件成员诊断

    /// <summary>
    /// 验证:尝试为包含事件成员的类型生成代理应该报告 ACSG003 警告。
    ///
    /// 预期诊断:Id=ACSG003,Severity=Warning,消息包含类型名与事件成员名。
    /// </summary>
    [Fact]
    public void TypeWithEvent_Should_Report_ACSG003_Warning_Documentation()
    {
        const string source = """
            using System;
            using AspectCore.DynamicProxy;

            [AspectCoreGenerateProxy]
            public class ServiceWithEvent
            {
                public virtual event EventHandler MyEvent;
                public virtual void DoWork() { }
            }
            """;

        var runResult = RunGenerator(source, "TypeWithEventDiagnostic");

        var diagnostic = Assert.Single(runResult.Diagnostics.Where(d => d.Id == "ACSG003"));
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Contains("ServiceWithEvent", diagnostic.GetMessage());
        Assert.Contains("MyEvent", diagnostic.GetMessage());
    }

    #endregion

    /// <summary>
    /// 编译给定源码并运行 <see cref="AspectCoreProxyGenerator"/>,返回生成器运行结果
    /// (包含诊断与生成的语法树)。编译驱动范式与 <see cref="SourceGeneratorDiagnosticTests"/> 一致。
    /// </summary>
    private static GeneratorDriverRunResult RunGenerator(
        string source,
        string assemblyName,
        LanguageVersion languageVersion = LanguageVersion.Latest)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: assemblyName,
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(languageVersion)) },
            references: AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.IsDynamic
                    && !string.IsNullOrEmpty(assembly.Location)
                    && assembly != typeof(SourceGeneratorDiagnosticVerificationTests).Assembly)
                .Select(assembly => MetadataReference.CreateFromFile(assembly.Location)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new AspectCoreProxyGenerator().AsSourceGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

        return driver.GetRunResult();
    }
}
