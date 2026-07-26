; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
ACSG001 | AspectCore.SourceGenerator | Warning | Open generic type is not supported for proxy generation.
ACSG002 | AspectCore.SourceGenerator | Warning | Nested type is not supported for proxy generation.
ACSG003 | AspectCore.SourceGenerator | Warning | Event member is not supported for proxy generation.
ACSG004 | AspectCore.SourceGenerator | Warning | Open generic method is not supported for proxy generation.
ACSG005 | AspectCore.SourceGenerator | Error | Cannot generate a proxy for a sealed type.
ACSG006 | AspectCore.SourceGenerator | Error | Type is not accessible to the source generator.
ACSG007 | AspectCore.SourceGenerator | Error | Type has no accessible constructor for a class proxy.
ACSG008 | AspectCore.SourceGenerator | Error | Cannot generate a proxy for a ref struct type.
ACSG009 | AspectCore.SourceGenerator | Warning | byref-like params parameter is not supported for proxy generation.
ACSG010 | AspectCore.SourceGenerator | Warning | byref-like parameter is not supported for proxy generation.
ACSG011 | AspectCore.SourceGenerator | Warning | byref-like return value is not supported for proxy generation.
ACSG0101 | AspectCore.SourceGenerator | Warning | Open generic method falls back to reflection for NativeAOT.
