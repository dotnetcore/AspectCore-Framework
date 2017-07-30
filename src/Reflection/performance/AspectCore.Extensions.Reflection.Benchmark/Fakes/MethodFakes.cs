using System.Runtime.CompilerServices;

namespace AspectCore.Extensions.Reflection.Benchmark.Fakes
{
    public class MethodFakes
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public object Call() => null;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static object StaticCall() => null;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public virtual object CallVirt() => null;
    }
}