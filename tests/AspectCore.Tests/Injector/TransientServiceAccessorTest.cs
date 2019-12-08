using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.DependencyInjection;
using Xunit;

namespace AspectCore.Tests.Injector
{
    public class TransientServiceAccessorTest : InjectorTestBase
    {
        [Fact]
        public void Get_Value()
        {
            var accessor1 = ServiceResolver.Resolve<ITransientServiceAccessor<ITransient>>();
            Assert.NotEqual(accessor1.Value, accessor1.Value);
            Assert.NotEqual(accessor1.Value, ServiceResolver.Resolve<ITransient>());
            using (var scope = ServiceResolver.CreateScope())
            {
                var accessor2= scope.Resolve<ITransientServiceAccessor<ITransient>>();
                Assert.Equal(accessor1, accessor2);
                Assert.NotEqual(accessor1.Value, accessor2.Value);
            }
        }
        protected override void ConfigureService(IServiceContext serviceContext)
        {
            serviceContext.AddType<ITransient, Transient>();
        }
    }
}