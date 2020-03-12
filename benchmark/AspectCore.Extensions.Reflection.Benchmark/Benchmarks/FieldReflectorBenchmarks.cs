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
    [AllStatisticsColumn]
    [MemoryDiagnoser]
    public class FieldReflectorBenchmarks
    {
        private readonly FieldInfo _staticField;
        private readonly FieldReflector _staticFieldReflector;

        private readonly FieldInfo _field;
        private readonly FieldFakes _instance;
        private readonly FieldReflector _fieldReflector;

        public FieldReflectorBenchmarks()
        {
            FieldFakes.StaticFiled = "StaticFiled";
            _staticField = typeof(FieldFakes).GetTypeInfo().GetField("StaticFiled");
            _staticFieldReflector = _staticField.GetReflector();
            _instance = new FieldFakes();
            _instance.InstanceField = "InstanceField";
            _field = typeof(FieldFakes).GetTypeInfo().GetField("InstanceField");
            _fieldReflector = _field.GetReflector();
        }

        [Benchmark]
        public object Native_Get_Field()
        {
            return _instance.InstanceField;
        }

        [Benchmark]
        public object Reflection_Get_Field()
        {
            return _field.GetValue(_instance);
        }

        [Benchmark]
        public object Reflector_Get_Field()
        {
            return _fieldReflector.GetValue(_instance);
        }

        [Benchmark]
        public object Native_Get_Static_Field()
        {
            return FieldFakes.StaticFiled;
        }

        [Benchmark]
        public object Reflection_Get_Static_Field()
        {
            return _staticField.GetValue(null);
        }

        [Benchmark]
        public object Reflector_Get_Static_Field()
        {
            return _staticFieldReflector.GetStaticValue();
        }
    }
}
