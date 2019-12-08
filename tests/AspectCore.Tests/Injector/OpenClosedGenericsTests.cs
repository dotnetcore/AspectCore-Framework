using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AspectCore.DependencyInjection;
using Xunit;

namespace AspectCore.Tests.Injector
{
    public class OpenClosedGenericsTests:InjectorTestBase
    {
        [Fact]
        public void ResolvesMixedOpenClosedGenericsAsEnumerable()
        {
       
            var enumerable = ServiceResolver.Resolve<IEnumerable<IFakeOpenGenericService<PocoClass>>>().ToArray();

            Assert.Equal(3, enumerable.Length);
        }

        protected override void ConfigureService(IServiceContext serviceContext)
        {
            var instance = new FakeOpenGenericService<PocoClass>(null);

            serviceContext.AddType<PocoClass, PocoClass>(Lifetime.Singleton);
            serviceContext.AddType(typeof(IFakeOpenGenericService<PocoClass>), typeof(FakeService), Lifetime.Singleton);
            serviceContext.AddType(typeof(IFakeOpenGenericService<>), typeof(FakeOpenGenericService<>), Lifetime.Singleton);
            serviceContext.AddInstance<IFakeOpenGenericService<PocoClass>>(instance);
        }
    }

   
}
