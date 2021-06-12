using AspectCore.Configuration;
using AspectCore.Extensions.Autofac;
using AspectCore.Extensions.Test.Fakes;
using Autofac;
using Xunit;

namespace AspectCoreTest.Autofac.Issues
{
    // https://github.com/dotnetcore/AspectCore-Framework/issues/261
    public class PropertyInjectorProxyWithVirtualTests
    {
        public class Foo
        {
            public Bar Bar { get; set; }
            [CacheInterceptor]
            public virtual void Do()
            {
                Bar.Do();
            }
        }

        public class Bar
        {
            public void Do()
            {
            }
        }

        private ContainerBuilder CreateBuilder()
        {
            return new ContainerBuilder().RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate(next => ctx => next(ctx), Predicates.ForNameSpace("AspectCore.Extensions.Test.Issues"));
            });
        }

        [Fact]
        public void PropertyInjectorProxyWithVirtual_Test()
        {
            var builder = CreateBuilder();
            builder.RegisterType<Bar>().AsSelf();
            builder.RegisterType<Foo>().AsSelf().PropertiesAutowired();
            var container = builder.Build();
            var foo = container.Resolve<Foo>();
            foo.Do();
        }
    }
}