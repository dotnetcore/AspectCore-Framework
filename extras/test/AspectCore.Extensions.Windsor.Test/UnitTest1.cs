using System;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Extensions.Windsor.Test
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            Castle.Windsor.IWindsorContainer windsorContainer = new Castle.Windsor.WindsorContainer();
            windsorContainer.AddAspectCoreFacility();
            windsorContainer.Register(Castle.MicroKernel.Registration.Component.For<IService>().ImplementedBy<Service>());
            var s = windsorContainer.Resolve<IService>();
            s.Foo();
        }
    }

    public interface IService
    {
        void Foo();
    }

    public class Service : IService
    {
        [Intercept]
        public void Foo()
        {
            
        }
    }

    public class Intercept : AspectCore.DynamicProxy.AbstractInterceptorAttribute
    {
        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            return context.Invoke(next);
        }
    }
}
