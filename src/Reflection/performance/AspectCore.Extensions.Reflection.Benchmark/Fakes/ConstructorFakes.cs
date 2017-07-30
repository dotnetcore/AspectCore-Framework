using System.Runtime.CompilerServices;

namespace AspectCore.Extensions.Reflection.Benchmark.Fakes
{
    public class ConstructorFakes
    {
        public string Name { get; set; }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public ConstructorFakes()
        {
            //Name = "Nonparametric constructor";
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public ConstructorFakes(string name)
        {
            Name = "Parametric constructor. param : " + name;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public ConstructorFakes(ref string name, ref ConstructorFakes fakes)
        {
            name = Name = "Parametric constructor with ref param.";
            fakes = new ConstructorFakes();
        }
    }
}
