using System.Threading.Tasks;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.Extensions.DependencyInjection.Test;

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
        services.AddKeyedScoped<IKeydService, KeydService>("key1");
        services.AddKeyedScoped<IKeydService, KeydService>("key2");
        var serviceProvider = services.BuildDynamicProxyProvider();
        var keydService = serviceProvider.GetKeyedService<IKeydService>("key1");
        Assert.Equal(1, keydService.Get());
        Assert.Equal(1000, keydService.GetIntercept());

        var keyd2Service = serviceProvider.GetKeyedService<IKeydService>("key2");
        //不同实例
        Assert.Equal(1, keyd2Service.Get());
        Assert.Equal(1000, keyd2Service.GetIntercept());
    }

    [Fact]
    public void GetKeydService_WithServiceContext()
    {
        // Regression test for #326: keyed services should not throw when using
        // UseServiceContext / ToServiceContext path.
        var services = new ServiceCollection();
        services.AddKeyedScoped<IKeydService, KeydService>("key1");
        services.AddKeyedScoped<IKeydService, KeydService>("key2");

        // This used to throw:
        //   System.InvalidOperationException: This service descriptor is keyed.
        //   Your service provider may not support keyed services.
        var serviceContext = services.ToServiceContext();
        var serviceProvider = serviceContext.Build();

        // Resolve via the AspectCore container (key info is not preserved,
        // so we verify the service is at least resolvable by its service type).
        var keydService = serviceProvider.GetService<IKeydService>();
        Assert.NotNull(keydService);
        Assert.Equal(1, keydService.Get());
        Assert.Equal(1000, keydService.GetIntercept());
    }
#endif
}