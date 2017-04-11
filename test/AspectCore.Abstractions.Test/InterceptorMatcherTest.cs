using AspectCore.Abstractions.Internal;
using AspectCore.Abstractions.Internal.Test.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace AspectCore.Abstractions.Internal.Test
{
    public class InterceptorMatcherTest
    {
        [Fact]
        public void WithOutInterceptor_Test()
        {
            var Configure = new AspectConfigure();

            var matcher = new InterceptorMatcher(Configure);

            var method = ReflectionExtensions.GetMethod<Action<InterceptorMatcherModel>>(m => m.WithOutInterceptor());

            var interceptors = matcher.Select(method, method.DeclaringType.GetTypeInfo());

            Assert.Empty(interceptors);
        }

        [Fact]
        public void With_Configure_Interceptor_Test()
        {
            var Configure = new AspectConfigure();

            var ConfigureInterceptor = new InjectedInterceptor();
            Configure.GetConfigureOption<IInterceptor>().Add(m => ConfigureInterceptor);

            var matcher = new InterceptorMatcher(Configure);
            var method = ReflectionExtensions.GetMethod<Action<InterceptorMatcherModel>>(m => m.ConfigureInterceptor());

            var interceptors = matcher.Select(method, method.DeclaringType.GetTypeInfo());

            Assert.NotEmpty(interceptors);

            Assert.Single(interceptors, ConfigureInterceptor);
        }

        [Fact]
        public void With_Method_Interceptor_Test()
        {
            var Configure = new AspectConfigure();

            var matcher = new InterceptorMatcher(Configure);
            var method = ReflectionExtensions.GetMethod<Action<InterceptorMatcherModel>>(m => m.WithInterceptor());

            var interceptors = matcher.Select(method, method.DeclaringType.GetTypeInfo());

            Assert.NotEmpty(interceptors);
            Assert.Single(interceptors);
        }


        [Fact]
        public void With_Type_Interceptor_Test()
        {
            var Configure = new AspectConfigure();

            var matcher = new InterceptorMatcher(Configure);
            var method = ReflectionExtensions.GetMethod<Action<WithInterceptorMatcherModel>>(m => m.WithOutInterceptor());

            var interceptors = matcher.Select(method, typeof(WithInterceptorMatcherModel).GetTypeInfo());

            Assert.NotEmpty(interceptors);
            Assert.Single(interceptors);
        }
    }
}
