using System;
using System.Reflection;
using AspectCore.Extensions.Reflection.Benchmark.Fakes;
using BenchmarkDotNet.Attributes;

namespace AspectCore.Extensions.Reflection.Benchmark.Benchmarks
{
    [AllStatisticsColumn]
    [MemoryDiagnoser]
    public class PropertyReflectorBenchmarks
    {
        private readonly PropertyInfo _staticField;
        private readonly PropertyReflector _staticFieldReflector;

        private readonly PropertyInfo _field;
        private readonly PropertyFakes _instance;
        private readonly PropertyReflector _fieldReflector;

        public PropertyReflectorBenchmarks()
        {
            PropertyFakes.StaticProperty = "StaticProperty";
            _staticField = typeof(PropertyFakes).GetTypeInfo().GetProperty("StaticProperty");
            _staticFieldReflector = _staticField.GetReflector();
            _instance = new PropertyFakes();
            _instance.InstanceProperty = "InstanceProperty";
            _field = typeof(PropertyFakes).GetTypeInfo().GetProperty("InstanceProperty");
            _fieldReflector = _field.GetReflector();
        }

        [Benchmark]
        public object Native_Get_Property()
        {
            return _instance.InstanceProperty;
        }

        //[Benchmark]
        public object Reflection_Get_Property()
        {
            return _field.GetValue(_instance);
        }

        [Benchmark]
        public object AspectCore_Reflector_Get_Property()
        {
            return _fieldReflector.GetValue(_instance);
        }


        //[Benchmark]
        //public object Native_Get_Static_Property()
        //{
        //    return PropertyFakes.StaticProperty;
        //}

        //[Benchmark]
        //public object Reflection_Get_Static_Property()
        //{
        //    return _staticField.GetValue(null);
        //}

        [Benchmark]
        public object AspectCore_Reflector_Get_Static_Property()
        {
            return _staticFieldReflector.GetStaticValue();
        }
    }
}
