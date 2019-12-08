using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.AspectScope;
using AspectCore.Injector;

namespace AspectScope.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            IServiceContainer serviceContainer = new ServiceContainer();
            serviceContainer.AddAspectScope();
            serviceContainer.AddType<IA, A>();
            serviceContainer.AddType<IB, B>();
            serviceContainer.AddType<IC, C>();
            var r = serviceContainer.Build();
            Task.WaitAll(
                Task.Run(() => { r.CreateScope().Resolve<IA>().None(); }),
            Task.Run(() => { r.CreateScope().Resolve<IB>().None(); }),
            Task.Run(() => { r.CreateScope().Resolve<IC>().None(); }),
            Task.Run(() => { r.CreateScope().Resolve<IA>().Nested(); }),
            Task.Run(() => { r.CreateScope().Resolve<IB>().Nested(); }),
            Task.Run(() => { r.CreateScope().Resolve<IC>().Nested(); }),
            Task.Run(() => { r.CreateScope().Resolve<IA>().Aspect(); }),
            Task.Run(() => { r.CreateScope().Resolve<IB>().Aspect(); }),
            Task.Run(() => { r.CreateScope().Resolve<IC>().Aspect(); }));

            IServiceContainer serviceContainer1 = new ServiceContainer();
            serviceContainer1.AddAspectScope();
            serviceContainer1.AddType<IA, A>();
            serviceContainer1.AddType<IB, B>();
            serviceContainer1.AddType<IC, C>();

            Console.WriteLine();

            var r1 = serviceContainer1.Build();
            r1.Resolve<IA>().None();
            r1.Resolve<IB>().None();
            r1.Resolve<IC>().None();
            r1.Resolve<IA>().Nested();
            r1.Resolve<IB>().Nested();
            r1.Resolve<IC>().Nested();
            r1.Resolve<IA>().Aspect();
            r1.Resolve<IB>().Aspect();
            r1.Resolve<IC>().Aspect();

            Console.ReadKey();
        }
    }

    
    public interface IA
    {
        [ScopeIntercept(Scope=Scope.None)]
        void None();

        [ScopeIntercept(Scope = Scope.Nested)]
        void Nested();

        [ScopeIntercept(Scope = Scope.Aspect)]
        void Aspect();
    }

    public interface IB
    {
        [ScopeIntercept(Scope = Scope.None)]
        void None();

        [ScopeIntercept(Scope = Scope.Nested)]
        void Nested();

        [ScopeIntercept(Scope = Scope.Aspect)]
        void Aspect();
    }

    public interface IC
    {
        [ScopeIntercept(Scope = Scope.None)]
        void None();

        [ScopeIntercept(Scope = Scope.Nested)]
        void Nested();

        [ScopeIntercept(Scope = Scope.Aspect)]
        void Aspect();
    }

    public class A : IA
    {
        private readonly IB b;
        private readonly IC c;

        public A(IB b, IC c)
        {
            this.b = b;
            this.c = c;
        }

        public void Aspect()
        {
            b.Aspect();
        }

        public void Nested()
        {
            b.Nested();
        }

        public void None()
        {
            b.None();
        }
    }

    public class B : IB
    {
        private readonly IC c;

        public B(IC c)
        {
            this.c = c;
        }

        public void None()
        {
            c.None();
        }

        public void Nested()
        {
            c.Nested();
        }

        public void Aspect()
        {
            c.Aspect();
        }
    }

    public class C : IC
    {
        public void None()
        {
        }

        public void Nested()
        {
        }

        public void Aspect()
        {
        }
    }

    public class ScopeIntercept : ScopeInterceptorAttribute
    {
        public override Scope Scope { get; set; } = Scope.None;

        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            Console.WriteLine("trace id: {0} . execute method : {1}.{2}", GetTraceId(context), context.ServiceMethod.DeclaringType, context.ServiceMethod.Name);
            return context.Invoke(next);
        }

        private string GetTraceId(AspectContext currentContext)
        {
            var scheduler = (IAspectScheduler)currentContext.ServiceProvider.GetService(typeof(IAspectScheduler));
            var firstContext = scheduler.GetCurrentContexts().First();
            if (firstContext.AdditionalData.TryGetValue("trace-id", out var traceId))
            {
                return traceId.ToString();
            }
            traceId = Guid.NewGuid();
            firstContext.AdditionalData["trace-id"] = traceId;
            return traceId.ToString();
        }
    }
}