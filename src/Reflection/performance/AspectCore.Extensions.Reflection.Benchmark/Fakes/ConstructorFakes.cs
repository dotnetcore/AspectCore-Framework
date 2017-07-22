using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Extensions.Reflection.Benchmark.Fakes
{
    public class ConstructorFakes
    {
        public string Name { get; set; }
        public ConstructorFakes()
        {
            Name = "Nonparametric constructor";
        }

        public ConstructorFakes(string name)
        {
            Name = "Parametric constructor. param : " + name;
        }

        public ConstructorFakes(ref string name, ref ConstructorFakes fakes)
        {
            name = Name = "Parametric constructor with ref param.";
            fakes = new ConstructorFakes();
        }
    }
}
