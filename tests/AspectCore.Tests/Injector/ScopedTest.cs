using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.DependencyInjection;
using Xunit;

namespace AspectCore.Tests.Injector
{
    public class ScopedTest : InjectorTestBase
    {
        [Fact]
        public void Equal()
        {
            var scoped1 = ServiceResolver.Resolve<IScoped>();
            using(var scopedResolver = ServiceResolver.CreateScope())
            {
                var scoped2 = scopedResolver.Resolve<IScoped>();
                var scoped3 = scopedResolver.Resolve<IScoped>();

                var scoped4 = ServiceResolver.Resolve<IScoped>();

                Assert.Equal(scoped1, scoped4);
                Assert.Equal(scoped2, scoped3);
                Assert.NotEqual(scoped1, scoped2);
                Assert.NotEqual(scoped1, scoped3);
                Assert.NotEqual(scoped2, scoped4);
                Assert.NotEqual(scoped3, scoped4);
            }
        }

        [Fact]
        public void Dispose()
        {
            var scopedResolver = ServiceResolver.CreateScope();
            var scoped = scopedResolver.Resolve<IScoped>();
            scopedResolver.Dispose();
            Assert.True(scoped.IsDisposed);
        }


        protected override void ConfigureService(IServiceContext serviceContext)
        {
            serviceContext.Scopeds.AddType<IScoped, Scoped>();
        }
    }
}
