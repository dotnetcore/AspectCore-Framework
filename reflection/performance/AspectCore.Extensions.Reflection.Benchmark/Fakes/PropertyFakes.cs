using System.Runtime.CompilerServices;

namespace AspectCore.Extensions.Reflection.Benchmark.Fakes
{
    public class PropertyFakes
    {
        public static string StaticProperty { [MethodImpl(MethodImplOptions.NoInlining)]get; [MethodImpl(MethodImplOptions.NoInlining)]set; }

        public string InstanceProperty { [MethodImpl(MethodImplOptions.NoInlining)]get; [MethodImpl(MethodImplOptions.NoInlining)]set; }
    }
}