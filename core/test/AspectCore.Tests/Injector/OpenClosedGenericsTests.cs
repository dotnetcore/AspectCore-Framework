using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AspectCore.Injector;
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

        protected override void ConfigureService(IServiceContainer serviceContainer)
        {
            var instance = new FakeOpenGenericService<PocoClass>(null);

            serviceContainer.AddType<PocoClass, PocoClass>(Lifetime.Singleton);
            serviceContainer.AddType(typeof(IFakeOpenGenericService<PocoClass>), typeof(FakeService), Lifetime.Singleton);
            serviceContainer.AddType(typeof(IFakeOpenGenericService<>), typeof(FakeOpenGenericService<>), Lifetime.Singleton);
            serviceContainer.AddInstance<IFakeOpenGenericService<PocoClass>>(instance);
        }
    }

   
}
