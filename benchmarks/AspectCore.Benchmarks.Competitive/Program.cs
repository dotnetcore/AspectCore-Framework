using BenchmarkDotNet.Running;

namespace AspectCore.Benchmarks.Competitive;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== AspectCore vs Castle DynamicProxy: Competitive Benchmarks ===");
        Console.WriteLine();

        var switcher = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly);
        switcher.Run(args);
    }
}
