using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AspectCore.Extensions.Reflection.Benchmark.Fakes;
using BenchmarkDotNet.Attributes;

namespace AspectCore.Extensions.Reflection.Benchmark.Benchmarks
{
    [MemoryDiagnoser]
    public class FieldReflectorBenchmarks
    {
        private readonly FieldInfo _field;
        private readonly FieldReflector _fieldReflector;

        public FieldReflectorBenchmarks()
        {
            FieldFakes.StaticFiled = "StaticFiled";
            _field = typeof(FieldFakes).GetTypeInfo().GetField("StaticFiled");
            _fieldReflector = _field.AsReflector();
        }

        [Benchmark]
        public object Native_Get_Static_Field()
        {
            return FieldFakes.StaticFiled;
        }

        [Benchmark]
        public object Reflection_Get_Static_Field()
        {
            return _field.GetValue(null);
        }

        [Benchmark]
        public object Reflector_Get_Static_Field()
        {
            return _fieldReflector.GetStaticValue();
        }
    }
}
