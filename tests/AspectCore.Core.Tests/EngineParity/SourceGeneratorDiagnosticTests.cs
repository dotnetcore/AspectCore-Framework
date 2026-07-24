#nullable enable

using System;
using System.Linq;
using AspectCore.SourceGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace AspectCore.Core.Tests.EngineParity;

public class SourceGeneratorDiagnosticTests
{
    [Fact]
    public void SealedRecord_ReportsACSG005()
    {
        const string source = """
            using AspectCore.DynamicProxy;

            [AspectCoreGenerateProxy]
            public sealed record SealedRecordService(string Name)
            {
                public string Echo() => Name;
            }
            """;

        var compilation = CSharpCompilation.Create(
            assemblyName: "SealedRecordDiagnostic",
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp9)) },
            references: AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.IsDynamic
                    && !string.IsNullOrEmpty(assembly.Location)
                    && assembly != typeof(SourceGeneratorDiagnosticTests).Assembly)
                .Select(assembly => MetadataReference.CreateFromFile(assembly.Location)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new AspectCoreProxyGenerator().AsSourceGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var generatorDiagnostics);

        var diagnostic = Assert.Single(generatorDiagnostics.Where(d => d.Id == "ACSG005"));
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("SealedRecordService", diagnostic.GetMessage());
    }

    [Fact]
    public void ReadOnlySpanParameter_ReportsACSG010()
    {
        const string source = """
            using System;
            using AspectCore.DynamicProxy;

            [AspectCoreGenerateProxy]
            public class ReadOnlySpanParameterService
            {
                public virtual int Length(ReadOnlySpan<int> values) => values.Length;
            }
            """;

        var compilation = CSharpCompilation.Create(
            assemblyName: "ReadOnlySpanParameterDiagnostic",
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp7_3)) },
            references: AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.IsDynamic
                    && !string.IsNullOrEmpty(assembly.Location)
                    && assembly != typeof(SourceGeneratorDiagnosticTests).Assembly)
                .Select(assembly => MetadataReference.CreateFromFile(assembly.Location)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new AspectCoreProxyGenerator().AsSourceGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var generatorDiagnostics);

        var diagnostic = Assert.Single(generatorDiagnostics.Where(d => d.Id == "ACSG010"));
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Equal("values", diagnostic.Location.SourceTree?.GetText().ToString(diagnostic.Location.SourceSpan));
        Assert.Contains("ReadOnlySpanParameterService.Length", diagnostic.GetMessage());
        Assert.Contains("values", diagnostic.GetMessage());
    }

    [Fact]
    public void ReadOnlySpanReturn_ReportsACSG011()
    {
        const string source = """
            using System;
            using AspectCore.DynamicProxy;

            [AspectCoreGenerateProxy]
            public class ReadOnlySpanReturnService
            {
                public virtual ReadOnlySpan<int> GetValues(int[] values) => values;
            }
            """;

        var compilation = CSharpCompilation.Create(
            assemblyName: "ReadOnlySpanReturnDiagnostic",
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp7_3)) },
            references: AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.IsDynamic
                    && !string.IsNullOrEmpty(assembly.Location)
                    && assembly != typeof(SourceGeneratorDiagnosticTests).Assembly)
                .Select(assembly => MetadataReference.CreateFromFile(assembly.Location)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new AspectCoreProxyGenerator().AsSourceGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var generatorDiagnostics);

        var diagnostic = Assert.Single(generatorDiagnostics.Where(d => d.Id == "ACSG011"));
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Contains("ReadOnlySpanReturnService.GetValues", diagnostic.GetMessage());
        Assert.Contains("System.ReadOnlySpan<int>", diagnostic.GetMessage());
    }

    [Fact]
    public void NativeAotSignatureDiagnosticRules_NoByRefLikeSignature_ReturnsNone()
    {
        const string source = """
            public class PlainService
            {
                public int Add(int left, int right) => left + right;
            }
            """;

        var method = GetSingleMethod(source, "PlainService", "Add", LanguageVersion.CSharp7_3);

        var diagnostic = NativeAotSignatureDiagnosticRules.Analyze(method);

        Assert.False(diagnostic.HasDiagnostic);
        Assert.Equal(NativeAotSignatureDiagnosticKind.None, diagnostic.Kind);
        Assert.Same(method, diagnostic.Method);
        Assert.Null(diagnostic.Parameter);
    }

    [Fact]
    public void NativeAotSignatureDiagnosticRules_ByRefLikeParameter_ReturnsParameterDiagnostic()
    {
        const string source = """
            using System;

            public class SpanParameterService
            {
                public int Length(ReadOnlySpan<int> values) => values.Length;
            }
            """;

        var method = GetSingleMethod(source, "SpanParameterService", "Length", LanguageVersion.CSharp7_3);

        var diagnostic = NativeAotSignatureDiagnosticRules.Analyze(method);

        Assert.True(diagnostic.HasDiagnostic);
        Assert.Equal(NativeAotSignatureDiagnosticKind.ByRefLikeParameter, diagnostic.Kind);
        Assert.Same(method, diagnostic.Method);
        Assert.Equal("values", diagnostic.Parameter?.Name);
    }

    [Fact]
    public void NativeAotSignatureDiagnosticRules_ByRefLikeReturn_TakesPrecedenceOverParameter()
    {
        const string source = """
            using System;

            public class SpanReturnService
            {
                public ReadOnlySpan<int> Slice(ReadOnlySpan<int> values) => values;
            }
            """;

        var method = GetSingleMethod(source, "SpanReturnService", "Slice", LanguageVersion.CSharp7_3);

        var diagnostic = NativeAotSignatureDiagnosticRules.Analyze(method);

        Assert.True(diagnostic.HasDiagnostic);
        Assert.Equal(NativeAotSignatureDiagnosticKind.ByRefLikeReturn, diagnostic.Kind);
        Assert.Same(method, diagnostic.Method);
        Assert.Null(diagnostic.Parameter);
    }

#if NET10_0_OR_GREATER
    [Fact]
    public void NativeAotSignatureDiagnosticRules_ByRefLikeParamsParameter_ReturnsParamsDiagnostic()
    {
        const string source = """
            using System;

            public class SpanParamsService
            {
                public int Length(params ReadOnlySpan<int> values) => values.Length;
            }
            """;

        var method = GetSingleMethod(source, "SpanParamsService", "Length", LanguageVersion.CSharp13);

        var diagnostic = NativeAotSignatureDiagnosticRules.Analyze(method);

        Assert.True(diagnostic.HasDiagnostic);
        Assert.Equal(NativeAotSignatureDiagnosticKind.ByRefLikeParamsParameter, diagnostic.Kind);
        Assert.Same(method, diagnostic.Method);
        Assert.Equal("values", diagnostic.Parameter?.Name);
        Assert.True(diagnostic.Parameter?.IsParams);
    }
#endif

#if NET10_0_OR_GREATER
    [Fact]
    public void ParamsReadOnlySpanIndexer_ReportsACSG009OnIndexerParameter()
    {
        const string source = """
            using System;
            using AspectCore.DynamicProxy;

            [AspectCoreGenerateProxy]
            public class ParamsReadOnlySpanIndexerService
            {
                public virtual int this[params ReadOnlySpan<int> values] => values.Length;
            }
            """;

        var compilation = CSharpCompilation.Create(
            assemblyName: "ParamsReadOnlySpanIndexerDiagnostic",
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp13)) },
            references: AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.IsDynamic
                    && !string.IsNullOrEmpty(assembly.Location)
                    && assembly != typeof(SourceGeneratorDiagnosticTests).Assembly)
                .Select(assembly => MetadataReference.CreateFromFile(assembly.Location)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new AspectCoreProxyGenerator().AsSourceGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var generatorDiagnostics);

        var diagnostic = Assert.Single(generatorDiagnostics.Where(d => d.Id == "ACSG009"));
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Equal("values", diagnostic.Location.SourceTree?.GetText().ToString(diagnostic.Location.SourceSpan));
        Assert.Contains("ParamsReadOnlySpanIndexerService.this", diagnostic.GetMessage());
        Assert.Contains("values", diagnostic.GetMessage());
    }
#endif

    private static IMethodSymbol GetSingleMethod(string source, string typeName, string methodName, LanguageVersion languageVersion)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: $"{typeName}DiagnosticRules",
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(languageVersion)) },
            references: AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.IsDynamic
                    && !string.IsNullOrEmpty(assembly.Location)
                    && assembly != typeof(SourceGeneratorDiagnosticTests).Assembly)
                .Select(assembly => MetadataReference.CreateFromFile(assembly.Location)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var type = compilation.GlobalNamespace
            .GetTypeMembers(typeName)
            .Single();

        return type.GetMembers(methodName)
            .OfType<IMethodSymbol>()
            .Single(m => m.MethodKind == MethodKind.Ordinary);
    }
}
