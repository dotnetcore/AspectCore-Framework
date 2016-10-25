using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Test.Fakes;
using System.Reflection;

namespace AspectCore.Lite.Test
{
    public class InterceptorMatcherTest : IDependencyInjection
    {
        private readonly IServiceProvider serviceProvider;

        public InterceptorMatcherTest()
        {
            serviceProvider = this.BuildServiceProvider();
        }

        [Fact]
        public void Match_Test()
        {
            var interceptorMatcher = serviceProvider.GetRequiredService<IInterceptorMatcher>();
            var typeInfo = typeof(IInterceptorMatcherTestService).GetTypeInfo();
            foreach (var method in typeInfo.DeclaredMethods)
            {
                var interceptors = interceptorMatcher.Match(method, typeInfo);
                Assert.IsType<EmptyInterceptorAttribute>(interceptors[0]);
                Assert.IsType<InterceptorMatcherTestAttribute>(interceptors[1]);
            }
        }

        [EmptyInterceptor]
        public interface IInterceptorMatcherTestService
        {
            [InterceptorMatcherTest]
            void Foo();
        }

        public class InterceptorMatcherTestAttribute: InterceptorAttribute
        {

        }
    }
}
