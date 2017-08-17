using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AspectCore.Abstractions;
using AspectCore.Core;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;

namespace AspectCore.Benchmark
{
    [MemoryDiagnoser]
    [AllStatisticsColumn]
    public class ActivatorBenchmarks
    {
        private readonly static IAspectContextFactory contextFactory = new AspectContextFactory(new ServiceProvider());
        private readonly static IAspectBuilderFactory aspectBuilderFactory = ProxyFactory.CreateAspectBuilderFactory();
        private readonly static IAspectActivatorFactory aspectActivatorFactory = ProxyFactory.CreateActivatorFactory();

        private readonly IService service = new Service();

        public ActivatorBenchmarks()
        {
        }

        [Benchmark]
        public AspectActivatorContext Create_AspectActivatorContext()
        {
            return new AspectActivatorContext( MethodConstants.serviceMethod, MethodConstants.targetMethod, MethodConstants.proxyMethod, service, this, null);
        }

        [Benchmark]
        public AspectContext Create_AspectContext()
        {
            var context = new AspectActivatorContext( MethodConstants.serviceMethod, MethodConstants.targetMethod, MethodConstants.proxyMethod, service, this, null);
            return contextFactory.CreateContext<object>(context);
        }

        //[Benchmark]
        public void Create_AspectContextThenDispose()
        {
            var context = new AspectActivatorContext(MethodConstants.serviceMethod, MethodConstants.targetMethod, MethodConstants.proxyMethod, service, this, null);
            using (var aspectContext = contextFactory.CreateContext<object>(context))
            {
            }
        }

        [Benchmark]
        public IAspectBuilder Create_AspectBuilder()
        {
            var context = new AspectActivatorContext(MethodConstants.serviceMethod, MethodConstants.targetMethod, MethodConstants.proxyMethod, service, this, null);
            var aspectContext = contextFactory.CreateContext<object>(context);
            return aspectBuilderFactory.Create(aspectContext);
        }

        [Benchmark]
        public AspectDelegate AspectBuilder_Build_Delegate()
        {
            var context = new AspectActivatorContext( MethodConstants.serviceMethod, MethodConstants.targetMethod, MethodConstants.proxyMethod, service, this, null);
            var aspectContext = contextFactory.CreateContext<object>(context);
            var aspectBuilder = aspectBuilderFactory.Create(aspectContext);
            return aspectBuilder.Build();
        }

        [Benchmark]
        public Task Invoke_WithoutReturn()
        {
            var context = new AspectActivatorContext(MethodConstants.serviceMethod, MethodConstants.targetMethod, MethodConstants.proxyMethod, service, this, null);
            var aspectContext = contextFactory.CreateContext<object>(context);
            var aspectBuilder = aspectBuilderFactory.Create(aspectContext);
            return aspectBuilder.Build()(aspectContext);
        }

        [Benchmark]
        public object Invoke_WithReturn()
        {
            var context = new AspectActivatorContext( MethodConstants.serviceMethod, MethodConstants.targetMethod, MethodConstants.proxyMethod, service, this, null);
            var aspectContext = contextFactory.CreateContext<object>(context);
            var aspectBuilder = aspectBuilderFactory.Create(aspectContext);
            var invoke = aspectBuilder.Build()(aspectContext);
            if (invoke.IsFaulted)
            {
                throw invoke.Exception?.InnerException;
            }
            if (!invoke.IsCompleted)
            {
                invoke.GetAwaiter().GetResult();
            }
            return aspectContext.ReturnValue;
        }

        [Benchmark]
        public IAspectActivator Create_AspectActivator()
        {
            return aspectActivatorFactory.Create();
        }

        [Benchmark]
        public object AspectActivator_Invoke()
        {
            var context = new AspectActivatorContext(MethodConstants.serviceMethod, MethodConstants.targetMethod, MethodConstants.proxyMethod, service, this, null);
            return aspectActivatorFactory.Create().Invoke<object>(context);
        }
    }

    public class MethodConstants
    {
        public static MethodInfo serviceMethod;
        public static MethodInfo targetMethod;
        public static MethodInfo proxyMethod;

        static MethodConstants()
        {
            serviceMethod = typeof(IService).GetMethod(nameof(IService.GetMessage));
            targetMethod = typeof(Service).GetMethod(nameof(Service.GetMessage));
            proxyMethod = typeof(Service).GetMethod(nameof(Service.GetMessage));
        }
    }
}