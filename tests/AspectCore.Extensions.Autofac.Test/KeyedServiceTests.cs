using System.Threading.Tasks;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Autofac;
using AspectCore.Extensions.DependencyInjection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCoreTest.Autofac;

public class KeyedServiceTests
{
    public class InterceptKey : AbstractInterceptorAttribute
    {
        private int _key;

        public InterceptKey(int key)
        {
            _key = key;
        }

        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            await context.Invoke(next);
            context.ReturnValue = _key;
        }
    }

    public interface IKeydService
    {
        int Get();
        int GetIntercept();
    }

    public class KeydService : IKeydService
    {
        private int _current = 0;
        public int Get()
        {
            _current++;
            return _current;
        }

        [InterceptKey(1000)]
        public int GetIntercept()
        {
            return 2;
        }
    }
#if NET8_0_OR_GREATER
    [Fact]
    public void GetKeydService_WithServiceProvider()
    {
        var services = new ServiceCollection();
        var builder = new ContainerBuilder();
        builder.RegisterDynamicProxy();
        services.AddKeyedScoped<IKeydService, KeydService>("key1");
        services.AddKeyedScoped<IKeydService, KeydService>("key2");
        builder.Populate(services);
        var serviceProvider = new AutofacServiceProvider(builder.Build());
        var keydService = serviceProvider.GetKeyedService<IKeydService>("key1");
        Assert.Equal(1, keydService.Get());
        Assert.Equal(1000, keydService.GetIntercept());

        var keyd2Service = serviceProvider.GetKeyedService<IKeydService>("key2");
        //不同实例
        Assert.Equal(1, keyd2Service.Get());
        Assert.Equal(1000, keyd2Service.GetIntercept());
    }
#endif
}