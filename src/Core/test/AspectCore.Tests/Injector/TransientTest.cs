using AspectCore.Injector;
using Xunit;

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
            Assert.IsType<Transient>(result);
        }

        [Fact]
        public void ResolveDelegate()
        {
            var result = ServiceResolver.Resolve<IDelegateTransient>();
            Assert.NotNull(result);
            Assert.IsType<Transient>(result);
        }

        protected override void ConfigureService(IServiceContainer serviceContainer)
        {
            serviceContainer.Transients.AddType<ITransient, Transient>();
            serviceContainer.Transients.AddDelegate<IDelegateTransient>(resolver => new Transient());
        }
    }
}
