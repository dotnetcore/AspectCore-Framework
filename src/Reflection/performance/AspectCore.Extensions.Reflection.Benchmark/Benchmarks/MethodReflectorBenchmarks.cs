using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AspectCore.Extensions.Reflection.Benchmark.Fakes;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;

namespace AspectCore.Extensions.Reflection.Benchmark
{
    [AllStatisticsColumn]
    [MemoryDiagnoser]
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

        private readonly Func<object, object[], object> _staticInvoker;
        private readonly Func<object, object[], object> _callInvoker;
        private readonly Func<object, object[], object> _callVirtualInvoker;

        private readonly MethodFakes _instance;

        public MethodReflectorBenchmarks()
        {
            var typeInfo = typeof(MethodFakes).GetTypeInfo();
            _method = typeInfo.GetMethod("Call");
            _staticMethod = typeInfo.GetMethod("StaticCall");
            _virtualMethod = typeInfo.GetMethod("CallVirt");
            _staticReflector = _staticMethod.AsReflector();
            _callReflectorWithCallOp = _method.AsReflector(CallOptions.Call);
            _callReflectorWithCallVirtOp = _method.AsReflector(CallOptions.Callvirt);
            _virtualReflectorWithCallOp = _virtualMethod.AsReflector(CallOptions.Call);
            _virtualReflectorWithCallVirtOp = _virtualMethod.AsReflector(CallOptions.Callvirt);

            _instance = new MethodFakes();
            _staticInvoker = _staticReflector.Invoker;
            _callInvoker = _callReflectorWithCallVirtOp.Invoke;
            _callVirtualInvoker = _virtualReflectorWithCallVirtOp.Invoke;
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
        public void Reflector_Invoker_CallWithCallVirtOp()
        {
            _callInvoker(_instance, null);
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
        public void Reflector_Invoker_CallVirtWithCallVirtOp()
        {
            _callVirtualInvoker(_instance, null);
        }

        [Benchmark]
        public object Native_StaticCall()
        {
            MethodFakes.StaticCall();
            return _instance;
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

        [Benchmark]
        public object Reflector_Invoker_StaticCall()
        {
            return _staticInvoker(null, null);
        }
    }
}