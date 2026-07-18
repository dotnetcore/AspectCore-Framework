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
}
