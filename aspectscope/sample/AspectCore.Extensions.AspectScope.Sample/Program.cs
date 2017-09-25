using System;
using System.Linq;
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
            var a = r.Resolve<IA>();
            a.Foo();
            a.Foo();
            Console.ReadKey();
        }
    }

    [ScopeIntercept]
    public interface IA
    {
        void Foo();
    }
    [ScopeIntercept]
    public interface IB
    {
        void Foo();
    }
    [ScopeIntercept]
    public interface IC
    {
        void Foo();
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
        public void Foo()
        {
            Console.WriteLine("A");
            b.Foo();
            c.Foo();
        }
    }

    public class B : IB
    {
        private readonly IC c;

        public B(IC c)
        {
            this.c = c;
        }

        public void Foo()
        {
            Console.WriteLine("B");
            c.Foo();
        }
    }

    public class C : IC
    {
        public void Foo()
        {
            Console.WriteLine("C");
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