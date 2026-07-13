using BenchmarkDotNet.Running;

namespace AspectCore.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== AspectCore Framework: SourceGenerator vs DynamicProxy Benchmarks ===");
        Console.WriteLine();

        var switcher = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly);
        switcher.Run(args);
    }
}
