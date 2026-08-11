#nullable enable

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using AspectCore.SourceGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace AspectCore.Core.Tests.EngineParity;

/// <summary>
/// Assembly-level <c>[assembly: AspectCoreGenerateProxy]</c> auto-discovery must actually
/// produce proxies. Regression coverage for the Execute gate that used to drop every
/// auto-discovered candidate (<c>attrData is null → continue</c>) — the auto-discovered
/// types carry no type-level attribute, so the gate silently produced zero proxies.
/// </summary>
public class AssemblyLevelAutoProxyTests
{
    private const string AssemblyLevelSource = """
        using AspectCore.DynamicProxy;

        [assembly: AspectCoreGenerateProxy]

        namespace AutoProxy
        {
            public class CalcService
            {
                public virtual int Add(int a, int b) => a + b;
            }

            public interface ICalc
            {
                int Add(int a, int b);
            }
        }
        """;

    [Fact]
    public void AssemblyLevelAttribute_AutoDiscoversClassAndInterface()
    {
        var (driver, generatorDiagnostics, compilationErrors) = RunGenerator(AssemblyLevelSource);

        var generatedSources = driver.GetRunResult().Results[0].GeneratedSources;

        // Class proxy for CalcService
        Assert.Contains(generatedSources, s => s.HintName.Contains("CalcService") && s.HintName.Contains("ClassProxy"));
        // No-target interface proxy for ICalc
        Assert.Contains(generatedSources, s => s.HintName.Contains("ICalc"));
        // A registry is emitted so the runtime can discover the proxies
        Assert.Contains(generatedSources, s => s.HintName == "AspectCoreSourceGeneratedProxyRegistry.g.cs");

        Assert.Empty(generatorDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Empty(compilationErrors);
    }

    private const string IneligibleSource = """
        using System;
        using AspectCore.DynamicProxy;

        [assembly: AspectCoreGenerateProxy]

        namespace AutoProxy
        {
            public sealed class SealedService { public void Foo() {} }
            public static class StaticService { public static void Foo() {} }
            public class WithEvent { public virtual void Foo() {} public event Action E; }
            public class NoMembers { }
            public struct ValueThing { public void Foo() {} }
            public class GoodService { public virtual void Foo() {} }
        }
        """;

    [Fact]
    public void AssemblyLevelAttribute_SkipsIneligibleTypes()
    {
        var (driver, _, compilationErrors) = RunGenerator(IneligibleSource);
        var generatedSources = driver.GetRunResult().Results[0].GeneratedSources;

        Assert.Contains(generatedSources, s => s.HintName.Contains("GoodService"));
        Assert.DoesNotContain(generatedSources, s => s.HintName.Contains("SealedService"));
        Assert.DoesNotContain(generatedSources, s => s.HintName.Contains("StaticService"));
        Assert.DoesNotContain(generatedSources, s => s.HintName.Contains("WithEvent"));
        Assert.DoesNotContain(generatedSources, s => s.HintName.Contains("NoMembers"));
        Assert.DoesNotContain(generatedSources, s => s.HintName.Contains("ValueThing"));
        Assert.Empty(compilationErrors);
    }

    private const string CoexistSource = """
        using AspectCore.DynamicProxy;

        [assembly: AspectCoreGenerateProxy]

        namespace AutoProxy
        {
            public class AutoService { public virtual void Foo() {} }

            [AspectCoreGenerateProxy]
            public class ExplicitService { public virtual void Bar() {} }
        }
        """;

    [Fact]
    public void AssemblyLevelAttribute_CoexistsWithExplicitTypeLevelAttribute()
    {
        var (driver, generatorDiagnostics, compilationErrors) = RunGenerator(CoexistSource);
        var generatedSources = driver.GetRunResult().Results[0].GeneratedSources;

        Assert.Contains(generatedSources, s => s.HintName.Contains("AutoService"));
        // ExplicitService is handled once via the explicit path and must not be
        // duplicated by auto-discovery (IsEligibleForAutoProxy skips attributed types).
        Assert.Single(generatedSources.Where(s => s.HintName.Contains("ExplicitService")));
        Assert.Empty(generatorDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Empty(compilationErrors);
    }

    [Fact]
    public void ReferencedAssemblyWithAssemblyLevelAttribute_AutoDiscoversItsTypes()
    {
        const string libSource = """
            using AspectCore.DynamicProxy;

            [assembly: AspectCoreGenerateProxy]

            namespace Lib
            {
                public class LibService
                {
                    public virtual void Foo() {}
                }

                public interface ILib
                {
                    void Foo();
                }
            }
            """;

        var references = CreateReferences();
        var libReference = CompileLibrary(libSource, "AutoProxyLib");

        // Main compilation does NOT declare the assembly-level attribute; it only references the lib.
        const string mainSource = """
            namespace Main
            {
                public class MainService { public virtual void Foo() {} }
            }
            """;

        var mainCompilation = CSharpCompilation.Create(
            assemblyName: "AutoProxyMain",
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText(mainSource, new CSharpParseOptions(LanguageVersion.CSharp13)) },
            references: references.Concat(new[] { libReference }),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new AspectCoreProxyGenerator().AsSourceGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(mainCompilation, out var outputCompilation, out var generatorDiagnostics);
        var generatedSources = driver.GetRunResult().Results[0].GeneratedSources;

        Assert.Contains(generatedSources, s => s.HintName.Contains("LibService"));
        Assert.Contains(generatedSources, s => s.HintName.Contains("ILib"));
        // Main assembly has no assembly-level attribute → its own types are NOT auto-discovered
        Assert.DoesNotContain(generatedSources, s => s.HintName.Contains("MainService"));
        Assert.Empty(generatorDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Empty(outputCompilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
    }

    [Fact]
    public void ReferencedAssemblyWithAssemblyLevelAttribute_SkipsInternalTypes()
    {
        const string libSource = """
            using AspectCore.DynamicProxy;

            [assembly: AspectCoreGenerateProxy]

            namespace Lib
            {
                public class PublicService
                {
                    public virtual void Foo() {}
                }

                internal class InternalService
                {
                    public virtual void Bar() {}
                }

                internal interface IInternalService
                {
                    void Baz();
                }
            }
            """;

        var references = CreateReferences();
        var libReference = CompileLibrary(libSource, "AutoProxyLibWithInternal");

        const string mainSource = """
            namespace Main
            {
                public class MainService { public virtual void Foo() {} }
            }
            """;

        var mainCompilation = CSharpCompilation.Create(
            assemblyName: "AutoProxyMainWithInternal",
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText(mainSource, new CSharpParseOptions(LanguageVersion.CSharp13)) },
            references: references.Concat(new[] { libReference }),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new AspectCoreProxyGenerator().AsSourceGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(mainCompilation, out var outputCompilation, out var generatorDiagnostics);
        var generatedSources = driver.GetRunResult().Results[0].GeneratedSources;

        // Public types of the referenced assembly are auto-discovered...
        Assert.Contains(generatedSources, s => s.HintName.Contains("PublicService"));
        // ...but internal types are NOT reachable from the consumer's generated code — proxying
        // them would emit CS0122 in the consuming project (regression for the referenced-assembly
        // assembly-level auto-discovery path).
        Assert.DoesNotContain(generatedSources, s => s.HintName.Contains("InternalService"));
        Assert.DoesNotContain(generatedSources, s => s.HintName.Contains("IInternalService"));
        Assert.Empty(generatorDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Empty(outputCompilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
    }

    [Fact]
    public void AssemblyLevelAttribute_SkipsInternalTypesInCurrentAssembly()
    {
        const string source = """
            using AspectCore.DynamicProxy;

            [assembly: AspectCoreGenerateProxy]

            namespace AutoProxy
            {
                internal class InternalService
                {
                    public virtual void Foo() {}
                }

                public class PublicService
                {
                    public virtual void Foo() {}
                }
            }
            """;

        var (driver, generatorDiagnostics, compilationErrors) = RunGenerator(source);
        var generatedSources = driver.GetRunResult().Results[0].GeneratedSources;

        // The generated proxy class is always public (ProxyEmitter hardcodes it), so internal
        // source types would produce CS0060 (inconsistent accessibility) — they are excluded from
        // auto-discovery just like in referenced assemblies. Public types are still auto-discovered.
        Assert.DoesNotContain(generatedSources, s => s.HintName.Contains("InternalService"));
        Assert.Contains(generatedSources, s => s.HintName.Contains("PublicService"));
        Assert.Empty(generatorDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Empty(compilationErrors);
    }

    private static MetadataReference CompileLibrary(string libSource, string assemblyName)
    {
        var libCompilation = CSharpCompilation.Create(
            assemblyName: assemblyName,
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText(libSource, new CSharpParseOptions(LanguageVersion.CSharp13)) },
            references: CreateReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var ms = new MemoryStream();
        var emitResult = libCompilation.Emit(ms);
        Assert.True(emitResult.Success, string.Join("\n", emitResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
        // CreateFromImage keeps the bytes alive independently of the stream lifetime.
        return MetadataReference.CreateFromImage(ms.ToArray());
    }

    private static (GeneratorDriver Driver, ImmutableArray<Diagnostic> GeneratorDiagnostics, ImmutableArray<Diagnostic> CompilationErrors) RunGenerator(string source)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "AutoProxyCompilation",
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp13)) },
            references: CreateReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new AspectCoreProxyGenerator().AsSourceGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generatorDiagnostics);

        var compilationErrors = outputCompilation
            .GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToImmutableArray();

        return (driver, generatorDiagnostics, compilationErrors);
    }

    private static System.Collections.Generic.List<MetadataReference> CreateReferences()
    {
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic
                && !string.IsNullOrEmpty(assembly.Location)
                && assembly != typeof(AssemblyLevelAutoProxyTests).Assembly)
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .ToList<MetadataReference>();

        // The runtime package defining [AspectCoreGenerateProxy] may not be loaded into the
        // test AppDomain yet; force it in so the generator can resolve the attribute symbol.
        var attributeAssembly = typeof(AspectCore.DynamicProxy.AspectCoreGenerateProxyAttribute).Assembly;
        if (references.All(r => r.Display != attributeAssembly.Location))
        {
            references.Add(MetadataReference.CreateFromFile(attributeAssembly.Location));
        }

        return references;
    }
}
