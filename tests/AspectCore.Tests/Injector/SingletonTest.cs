using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.DependencyInjection;
using Xunit;

namespace AspectCore.Tests.Injector
{
    public class SingletonTest : InjectorTestBase
    {
        [Fact]
        public void Equal()
        {
            var singleton1 = ServiceResolver.Resolve<ISingleton>();
            using (var scopedResolver = ServiceResolver.CreateScope())
            {
                var singleton2 = scopedResolver.Resolve<ISingleton>();
                using (var scopedResolver1 = ServiceResolver.CreateScope())
                {
                    var singleton3 = scopedResolver1.Resolve<ISingleton>();
                    Assert.Equal(singleton1, singleton2);
                    Assert.Equal(singleton1, singleton3);
                    Assert.Equal(singleton2, singleton3);
                }      
            }
        }

        [Fact]
        public void Dispose()
        {
            var scopedResolver = ServiceResolver.CreateScope();
            var singleton = ServiceResolver.Resolve<ISingleton>();
            scopedResolver.Dispose();
            Assert.False(singleton.IsDisposed);
            ServiceResolver.Dispose();
            Assert.True(singleton.IsDisposed);
        }

        protected override void ConfigureService(IServiceContext serviceContext)
        {
            serviceContext.Singletons.AddType<ISingleton, Singleton>();
        }
    }
}