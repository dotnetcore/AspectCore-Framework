using System.Threading.Tasks;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.Tests.DynamicProxy;
using Xunit;

namespace AspectCore.Tests.Issues.DynamicProxy
{
    // https://github.com/dotnetcore/AspectCore-Framework/issues/203
    public class NullableEnumWithDefaultValueTests : DynamicProxyTestBase
    {
        public class Interceptor : AbstractInterceptorAttribute
        {
            public override Task Invoke(AspectContext context, AspectDelegate next)
            {
                return context.Invoke(next);
            }
        }

        public class Service
        {
            [Interceptor]
            public virtual bool Get(Bool? flag = Bool.True)
            {
                return flag == Bool.True;
            }
        }

        public enum Bool
        {
            False,
            True
        }

        [Theory]
        [InlineData(Bool.True)]
        [InlineData(Bool.False)]
        public void ClassProxy_Test(Bool input)
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            var output = service.Get(input);
            Assert.Equal(input == Bool.True, output);
        }
    }
}
