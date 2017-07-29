using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectCore.Extensions.Reflection.Benchmark.Fakes
{
    public class MethodFakes
    {
        public object Call() => null;
        public static object StaticCall() => null;
        public virtual object CallVirt() => null;
    }
}
