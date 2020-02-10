using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.Extensions.DependencyInjection.Test.Issues
{
    // https://github.com/dotnetcore/AspectCore-Framework/issues/192
    public class InterceptorAttributeWithArrayMemberTests
    {
        public class TestAttribute : AbstractInterceptorAttribute
        {
            public int[] Times { get; set; } = { 1, 100, 10000, 1000000 };
            public override async Task Invoke(AspectContext context, AspectDelegate next)
            {
                foreach (var times in Times)
                {
                    for (var i = 0; i < times; i++)
                        await next(context);
                }
            }
        }

        public interface IUserAppService
        {
            int ExcuteTimes { get; }

            [Test(Times = new int[] { 10, 100 })]
            string DisplayName(string firstName, string lastName);
        }

        public class UserAppService : IUserAppService
        {
            private int _excuteTimes;
            public int ExcuteTimes => _excuteTimes;

            public string DisplayName(string firstName, string lastName)
            {
                Interlocked.Increment(ref _excuteTimes);
                var fullName = $"{firstName} {lastName}";
                return fullName;
            }
        }

        [Fact]
        public void InterceptorAttributeWithArrayMember_Property_Test()
        {
            var sp = new ServiceCollection()
                .AddScoped<IUserAppService, UserAppService>()
                .ConfigureDynamicProxy()
                .BuildDynamicProxyProvider();

            var usrAppSrv = sp.GetRequiredService<IUserAppService>();
            var name = usrAppSrv.DisplayName("gain", "loss");
            Assert.Equal("gain loss", name);
            Assert.Equal(10 + 100, usrAppSrv.ExcuteTimes);
        }
    }
}
