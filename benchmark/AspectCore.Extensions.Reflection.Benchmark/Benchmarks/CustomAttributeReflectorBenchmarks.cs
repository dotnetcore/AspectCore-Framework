using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace AspectCore.Extensions.Reflection.Benchmark.Benchmarks
{
    [AllStatisticsColumn]
    [MemoryDiagnoser]
    public class CustomAttributeReflectorBenchmarks
    {
        private readonly MethodInfo _method;
        private readonly MethodReflector _reflector;

        public CustomAttributeReflectorBenchmarks()
        {
            _method = typeof(CustomAttributeReflectorBenchmarks).GetMethod(nameof(Reflection_GetCustomAttribute));
            _reflector = _method.GetReflector();
        }

        [Attribute1]
        [Attribute2("benchmark", Id = 10000)]
        [Benchmark]
        [Attribute3]
        [Attribute3]
        [Attribute3]
        public Attribute Reflection_GetCustomAttribute()
        {
            return _method.GetCustomAttribute(typeof(Attribute2));
        }

        [Benchmark]
        public Attribute AspectCore_Reflector_GetCustomAttribute()
        {
            return _reflector.GetCustomAttribute(typeof(Attribute2));
        }

        [Benchmark]
        public IEnumerable<Attribute> Reflection_GetCustomAttributes_WithAttrType()
        {
            return _method.GetCustomAttributes(typeof(Attribute1));
        }

        [Benchmark]
        public IEnumerable<Attribute> AspectCore_Reflector_GetCustomAttributes_WithAttrType()
        {
            return _reflector.GetCustomAttributes(typeof(Attribute1));
        }

        [Benchmark]
        public IEnumerable<Attribute> Reflection_GetCustomAttributes_All()
        {
            return _method.GetCustomAttributes();
        }

        [Benchmark]
        public IEnumerable<Attribute> AspectCore_Reflector_GetCustomAttributes_All()
        {
            return _reflector.GetCustomAttributes();
        }

        [Benchmark]
        public bool Reflection_IsDefined()
        {
            return _method.IsDefined(typeof(Attribute3));
        }

        [Benchmark]
        public bool AspectCore_Reflector_IsDefined()
        {
            return _reflector.IsDefined(typeof(Attribute3));
        }
    }

    public class Attribute1 : Attribute
    { }

    public class Attribute2 : Attribute
    {
        public int Id { get; set; }

        public string Title { get; }

        public Attribute2(string title)
        {
            Title = title;
        }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class Attribute3 : Attribute1
    {
    }
}