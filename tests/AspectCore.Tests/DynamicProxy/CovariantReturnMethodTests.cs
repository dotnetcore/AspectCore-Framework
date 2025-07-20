using System.Linq;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Tests.DynamicProxy;

public class CovariantReturnMethodTests : DynamicProxyTestBase
{
    public class Interceptor : AbstractInterceptorAttribute
    {
        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            await context.Invoke(next);

            var returnType = context.ImplementationMethod.ReturnType;
            if (returnType == typeof(string))
            {
                context.ReturnValue += nameof(Interceptor);
            }
            else if (returnType == typeof(object))
            {
                context.ReturnValue = nameof(Interceptor);
            }
        }
    }

    public interface IService
    {
        object Property { get; }
        object Method();

        [Interceptor]
        object ProxyMethod();
    }

    public class Service : IService
    {
        public virtual object Property { get; } = 1;
        public virtual object Method() => 1;

        [Interceptor]
        public virtual object ProxyMethod() => new();
    }

    public class CovariantReturnsService : Service
    {
        public override string Property { get; } = nameof(CovariantReturnsService);
        public override string Method() => nameof(CovariantReturnsService);

        [Interceptor]
        public override string ProxyMethod() => nameof(CovariantReturnsService);
    }

    public class DerivedCovariantReturnsService : CovariantReturnsService
    {
        public override string Method() => nameof(DerivedCovariantReturnsService);

        [Interceptor]
        public override string ProxyMethod() => nameof(DerivedCovariantReturnsService);
    }

    [Fact]
    public void CreateClassProxy_Service_Test()
    {
        var service = ProxyGenerator.CreateClassProxy<Service>();
        Assert.Equal(1, service.Property);
        Assert.Equal(1, service.Method());
        Assert.Equal(nameof(Interceptor), service.ProxyMethod());
    }

    [Fact]
    public void CreateClassProxy_CovariantReturnsService_Test()
    {
        var service = ProxyGenerator.CreateClassProxy<CovariantReturnsService>();
        Assert.Equal(nameof(CovariantReturnsService), service.Property);
        Assert.Equal(nameof(CovariantReturnsService), service.Method());
        Assert.Equal(nameof(CovariantReturnsService) + nameof(Interceptor), service.ProxyMethod());
    }

    [Fact]
    public void CreateClassProxy_DerivedCovariantReturnsService_Test()
    {
        var service = ProxyGenerator.CreateClassProxy<DerivedCovariantReturnsService>();
        Assert.Equal(nameof(CovariantReturnsService), service.Property);
        Assert.Equal(nameof(DerivedCovariantReturnsService), service.Method());
        Assert.Equal(nameof(DerivedCovariantReturnsService) + nameof(Interceptor), service.ProxyMethod());
    }

    [Fact]
    public void CreateClassProxy_Service_CovariantReturnsService_Test()
    {
        var service = ProxyGenerator.CreateClassProxy<Service, CovariantReturnsService>();
        Assert.Equal(nameof(CovariantReturnsService), service.Property);
        Assert.Equal(nameof(CovariantReturnsService), service.Method());
        Assert.Equal(nameof(CovariantReturnsService) + nameof(Interceptor), service.ProxyMethod());
    }

    [Fact]
    public void CreateClassProxy_Service_DerivedCovariantReturnsService_Test()
    {
        var service = ProxyGenerator.CreateClassProxy<Service, DerivedCovariantReturnsService>();
        Assert.Equal(nameof(CovariantReturnsService), service.Property);
        Assert.Equal(nameof(DerivedCovariantReturnsService), service.Method());
        Assert.Equal(nameof(DerivedCovariantReturnsService) + nameof(Interceptor), service.ProxyMethod());
    }

    [Fact]
    public void CreateClassProxy_CovariantReturnsService_DerivedCovariantReturnsService_Test()
    {
        var service = ProxyGenerator.CreateClassProxy<CovariantReturnsService, DerivedCovariantReturnsService>();
        Assert.Equal(nameof(CovariantReturnsService), service.Property);
        Assert.Equal(nameof(DerivedCovariantReturnsService), service.Method());
        Assert.Equal(nameof(DerivedCovariantReturnsService) + nameof(Interceptor), service.ProxyMethod());
    }

    [Fact]
    public void CreateInterfaceProxy_IService_CovariantReturnsService_Test()
    {
        var service = ProxyGenerator.CreateInterfaceProxy<IService, CovariantReturnsService>();
        Assert.Equal(nameof(CovariantReturnsService), service.Property);
        Assert.Equal(nameof(CovariantReturnsService), service.Method());
        Assert.Equal(nameof(CovariantReturnsService) + nameof(Interceptor), service.ProxyMethod());
    }

    [Fact]
    public void CreateInterfaceProxy_IService_DerivedCovariantReturnsService_Test()
    {
        var methods = typeof(DerivedCovariantReturnsService).GetMethods()
            .Where(m => m.Name == nameof(IService.ProxyMethod))
            .ToArray();

        var service = ProxyGenerator.CreateInterfaceProxy<IService, DerivedCovariantReturnsService>();
        Assert.Equal(nameof(CovariantReturnsService), service.Property);
        Assert.Equal(nameof(DerivedCovariantReturnsService), service.Method());
        Assert.Equal(nameof(DerivedCovariantReturnsService) + nameof(Interceptor), service.ProxyMethod());
    }
}
