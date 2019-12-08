using AspectCore.DependencyInjection;
using Xunit;
using AspectCore.DynamicProxy;
using AspectCore.Configuration;

namespace AspectCore.Tests.Injector
{
    public class TransientTest : InjectorTestBase
    {
        [Fact]
        public void Equal()
        {
            var transient1 = ServiceResolver.Resolve<ITransient>();
            var transient2 = ServiceResolver.Resolve<ITransient>();
            using (var scopedResolver = ServiceResolver.CreateScope())
            {
                var transient3 = ServiceResolver.Resolve<ITransient>();
                Assert.NotEqual(transient1, transient2);
                Assert.NotEqual(transient2, transient3);
                Assert.NotEqual(transient1, transient3);
            }
        }

        [Fact]
        public void ResolveType()
        {
            var result = ServiceResolver.Resolve<ITransient>();
            Assert.NotNull(result);
        }

        [Fact]
        public void ResolveDelegate()
        {
            var result = ServiceResolver.Resolve<IDelegateTransient>();
            Assert.NotNull(result);
        }

        protected override void ConfigureService(IServiceContext serviceContext)
        {
            serviceContext.Transients.AddType<ITransient, Transient>();
            serviceContext.Transients.AddDelegate<IDelegateTransient, Transient>(resolver => new Transient());
        }
    }
}