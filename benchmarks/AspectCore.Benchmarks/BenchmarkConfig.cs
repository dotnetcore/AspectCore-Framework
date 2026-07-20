using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;

namespace AspectCore.Benchmarks;

/// <summary>
/// Default benchmark configuration for full-accuracy runs.
/// Includes memory diagnostics, statistical columns, and multiple exporters.
/// </summary>
public class DefaultConfig : ManualConfig
{
    public DefaultConfig()
    {
        AddDiagnoser(MemoryDiagnoser.Default);

        AddColumn(StatisticColumn.Mean);
        AddColumn(StatisticColumn.StdDev);
        AddColumn(StatisticColumn.Median);
        AddColumn(RankColumn.Arabic);
        AddColumn(BaselineRatioColumn.RatioMean);

        AddExporter(MarkdownExporter.GitHub);
        AddExporter(JsonExporter.Full);

        WithSummaryStyle(SummaryStyle.Default.WithRatioStyle(RatioStyle.Percentage));

        WithArtifactsPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..", "results",
                DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")));
    }
}

/// <summary>
/// Quick benchmark configuration using ShortRunJob for CI and fast iteration.
/// Trades accuracy for speed — suitable for regression detection, not precise measurement.
/// </summary>
public class QuickConfig : ManualConfig
{
    public QuickConfig()
    {
        AddJob(Job.ShortRun);

        AddDiagnoser(MemoryDiagnoser.Default);

        AddColumn(StatisticColumn.Mean);
        AddColumn(RankColumn.Arabic);
        AddColumn(BaselineRatioColumn.RatioMean);

        AddExporter(MarkdownExporter.GitHub);
        AddExporter(JsonExporter.Full);

        WithSummaryStyle(SummaryStyle.Default.WithRatioStyle(RatioStyle.Percentage));

        WithArtifactsPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..", "results",
                "ci-" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")));
    }
}
