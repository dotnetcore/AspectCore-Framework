using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AspectCore.Extensions.Reflection.Benchmark.Fakes;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace AspectCore.Extensions.Reflection.Benchmark
{
    [SimpleJob(RuntimeMoniker.NetCoreApp20)]
    [SimpleJob(RuntimeMoniker.NetCoreApp30)]
    [AllStatisticsColumn]
    public class MethodReflectorBenchmarks
    {
        private readonly MethodInfo _method;
        private readonly MethodInfo _virtualMethod;
        private readonly MethodInfo _staticMethod;

        private readonly MethodReflector _staticReflector;
        private readonly MethodReflector _callReflectorWithCallOp;
        private readonly MethodReflector _callReflectorWithCallVirtOp;
        private readonly MethodReflector _virtualReflectorWithCallOp;
        private readonly MethodReflector _virtualReflectorWithCallVirtOp;
        private readonly MethodFakes _instance;

        public MethodReflectorBenchmarks()
        {
            var typeInfo = typeof(MethodFakes).GetTypeInfo();
            _method = typeInfo.GetMethod("Call");
            _staticMethod = typeInfo.GetMethod("StaticCall");
            _virtualMethod = typeInfo.GetMethod("CallVirt");
            _staticReflector = _staticMethod.GetReflector();
            _callReflectorWithCallOp = _method.GetReflector(CallOptions.Call);
            _callReflectorWithCallVirtOp = _method.GetReflector(CallOptions.Callvirt);
            _virtualReflectorWithCallOp = _virtualMethod.GetReflector(CallOptions.Call);
            _virtualReflectorWithCallVirtOp = _virtualMethod.GetReflector(CallOptions.Callvirt);
            _instance = new MethodFakes();
        }

        [Benchmark]
        public void Native_Call()
        {
            _instance.Call();
        }

        [Benchmark]
        public void Reflection_Call()
        {
            _method.Invoke(_instance, null);
        }

        [Benchmark]
        public void Reflector_CallWithCallOp()
        {
            _callReflectorWithCallOp.Invoke(_instance);
        }

        [Benchmark]
        public void Reflector_CallWithCallVirtOp()
        {
            _callReflectorWithCallVirtOp.Invoke(_instance);
        }

        [Benchmark]
        public void Native_CallVirt()
        {
            _instance.CallVirt();
        }

        [Benchmark]
        public void Reflection_CallVirt()
        {
            _virtualMethod.Invoke(_instance, null);
        }

        [Benchmark]
        public void Reflector_CallVirtWithCallOp()
        {
            _virtualReflectorWithCallOp.Invoke(_instance);
        }

        [Benchmark]
        public void Reflector_CallVirtWithCallVirtOp()
        {
            _virtualReflectorWithCallVirtOp.Invoke(_instance);
        }

        [Benchmark]
        public object Native_StaticCall()
        {
            return MethodFakes.StaticCall();
        }

        [Benchmark]
        public object Reflection_StaticCall()
        {
            return _staticMethod.Invoke(null, null);
        }

        [Benchmark]
        public object Reflector_StaticCall()
        {
            return _staticReflector.StaticInvoke();
        }
    }
}