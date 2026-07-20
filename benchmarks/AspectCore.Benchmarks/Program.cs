using System.Text.Json;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace AspectCore.Benchmarks;

public class Program
{
    private static readonly string ResultsDir = Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", "results");

    private static readonly string BaselinePath = Path.Combine(ResultsDir, "baseline.json");

    public static int Main(string[] args)
    {
        Console.WriteLine("=== AspectCore Framework: SourceGenerator vs DynamicProxy Benchmarks ===");
        Console.WriteLine();

        // Handle custom modes
        if (args.Contains("--export-baseline"))
        {
            return RunExportBaseline(args);
        }

        if (args.Contains("--compare-baseline"))
        {
            return RunCompareBaseline(args);
        }

        // Determine config: --quick uses ShortRunJob config
        IConfig? config = null;
        var filteredArgs = args.ToList();
        if (filteredArgs.Remove("--quick"))
        {
            config = new QuickConfig();
        }

        var switcher = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly);

        if (config != null)
        {
            switcher.Run(filteredArgs.ToArray(), config);
        }
        else
        {
            switcher.Run(filteredArgs.ToArray());
        }

        return 0;
    }

    /// <summary>
    /// Runs all benchmarks with QuickConfig and exports results as baseline.json.
    /// </summary>
    private static int RunExportBaseline(string[] args)
    {
        Console.WriteLine("[Baseline Export] Running benchmarks with QuickConfig...");
        Console.WriteLine($"[Baseline Export] Results will be saved to: {BaselinePath}");
        Console.WriteLine();

        var config = new QuickConfig();
        var filteredArgs = args.Where(a => a != "--export-baseline").ToArray();

        var switcher = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly);
        var summaries = switcher.Run(filteredArgs, config);

        var baselineData = new Dictionary<string, BaselineEntry>();
        foreach (var summary in summaries)
        {
            foreach (var report in summary.Reports)
            {
                var key = report.BenchmarkCase.Descriptor.WorkloadMethod.DeclaringType?.Name
                    + "." + report.BenchmarkCase.Descriptor.WorkloadMethod.Name;

                if (report.ResultStatistics != null)
                {
                    baselineData[key] = new BaselineEntry
                    {
                        MeanNs = report.ResultStatistics.Mean,
                        AllocatedBytes = report.GcStats.GetBytesAllocatedPerOperation(report.BenchmarkCase) ?? 0,
                        Timestamp = DateTime.UtcNow.ToString("o")
                    };
                }
            }
        }

        Directory.CreateDirectory(Path.GetDirectoryName(BaselinePath)!);
        var json = JsonSerializer.Serialize(baselineData, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(BaselinePath, json);

        Console.WriteLine();
        Console.WriteLine($"[Baseline Export] Saved {baselineData.Count} entries to {BaselinePath}");
        return 0;
    }

    /// <summary>
    /// Runs benchmarks and compares against a saved baseline, reporting regressions.
    /// </summary>
    private static int RunCompareBaseline(string[] args)
    {
        if (!File.Exists(BaselinePath))
        {
            Console.Error.WriteLine($"[Compare] ERROR: Baseline file not found at {BaselinePath}");
            Console.Error.WriteLine("[Compare] Run with --export-baseline first to create a baseline.");
            return 1;
        }

        Console.WriteLine("[Compare] Loading baseline...");
        var baselineJson = File.ReadAllText(BaselinePath);
        var baseline = JsonSerializer.Deserialize<Dictionary<string, BaselineEntry>>(baselineJson)
            ?? new Dictionary<string, BaselineEntry>();

        Console.WriteLine($"[Compare] Loaded {baseline.Count} baseline entries.");
        Console.WriteLine("[Compare] Running current benchmarks...");
        Console.WriteLine();

        var config = new QuickConfig();
        var filteredArgs = args.Where(a => a != "--compare-baseline").ToArray();

        var switcher = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly);
        var summaries = switcher.Run(filteredArgs, config);

        // Compare results
        const double RegressionThreshold = 2.0; // 2x slower = regression
        var regressions = new List<string>();

        Console.WriteLine();
        Console.WriteLine("=== Baseline Comparison ===");
        var header = string.Format("{0,-60} {1,14} {2,14} {3,8} {4,10}",
            "Benchmark", "Baseline (ns)", "Current (ns)", "Ratio", "Status");
        Console.WriteLine(header);
        Console.WriteLine(new string('-', 110));

        foreach (var summary in summaries)
        {
            foreach (var report in summary.Reports)
            {
                var key = report.BenchmarkCase.Descriptor.WorkloadMethod.DeclaringType?.Name
                    + "." + report.BenchmarkCase.Descriptor.WorkloadMethod.Name;

                if (report.ResultStatistics == null || !baseline.ContainsKey(key))
                    continue;

                var baselineEntry = baseline[key];
                var currentMean = report.ResultStatistics.Mean;
                var ratio = currentMean / baselineEntry.MeanNs;
                var status = ratio > RegressionThreshold ? "REGRESSED" : "OK";

                Console.WriteLine($"{key,-60} {baselineEntry.MeanNs,14:F2} {currentMean,14:F2} {ratio,8:F2}x {status,10}");

                if (ratio > RegressionThreshold)
                {
                    regressions.Add($"{key}: {ratio:F2}x slower (baseline={baselineEntry.MeanNs:F0}ns, current={currentMean:F0}ns)");
                }
            }
        }

        Console.WriteLine();
        if (regressions.Count > 0)
        {
            Console.Error.WriteLine($"[Compare] FAILED: {regressions.Count} regression(s) detected (>{RegressionThreshold}x threshold):");
            foreach (var r in regressions)
                Console.Error.WriteLine($"  - {r}");
            return 1;
        }

        Console.WriteLine("[Compare] PASSED: No regressions detected.");
        return 0;
    }
}

internal sealed class BaselineEntry
{
    public double MeanNs { get; set; }
    public long AllocatedBytes { get; set; }
    public string Timestamp { get; set; } = "";
}
