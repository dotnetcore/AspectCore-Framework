using System;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Windsor;
using Castle.MicroKernel.Lifestyle;
using Castle.MicroKernel.Registration;
using Xunit;

namespace AspectCoreTest.Windsor
{
    public class ScopedServiceTests
    {
        [Fact]
        public void Test1()
        {
            Castle.Windsor.IWindsorContainer windsorContainer = new Castle.Windsor.WindsorContainer();
            windsorContainer.AddAspectCoreFacility(config =>
            {
                config.Interceptors.AddDelegate((ctx, next) =>
                {
                    var scopedService = ctx.ServiceProvider.GetService(typeof(ScopedService));
                    return ctx.Invoke(next);
                });
            });
            windsorContainer.AddAspectCoreFacility();
            windsorContainer.Register(Component.For<IService>().ImplementedBy<Service>());
            windsorContainer.Register(Component.For<ScopedService>().LifestyleScoped());
            windsorContainer.BeginScope();
            var s = windsorContainer.Resolve<IService>();
            s.Foo();
            windsorContainer.BeginScope();
            s = windsorContainer.Resolve<IService>();
            s.Foo();
        }
    }

    public interface IService
    {
        void Foo();
    }

    public class Service : IService
    {
        private readonly ScopedService scopedService;

        public Service(ScopedService scopedService)
        {
            this.scopedService = scopedService;
        }

        [Intercept]
        public  void Foo()
        {
            
        }
    }

    public class Intercept : AspectCore.DynamicProxy.AbstractInterceptorAttribute
    {
        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            var scopedService = context.ServiceProvider.GetService(typeof(ScopedService));
            return context.Invoke(next);
        }
    }

    public class ScopedService
    {
        public Guid Id { get; } = Guid.NewGuid();
    }
}
