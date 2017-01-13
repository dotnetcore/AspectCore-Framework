using AspectCore.Lite.Abstractions.Extensions;
using AspectCore.Lite.Abstractions.Resolution.Test.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace AspectCore.Lite.Abstractions.Resolution.Test
{
    public class InterceptorMatcherTest
    {
        [Fact]
        public void WithOutInterceptor_Test()
        {
            var configuration = new AspectConfiguration();

            var matcher = new InterceptorMatcher(configuration);

            var method = MethodInfoHelpers.GetMethod<Action<InterceptorMatcherModel>>(m => m.WithOutInterceptor());

            var interceptors = matcher.Match(method, method.DeclaringType.GetTypeInfo());

            Assert.Empty(interceptors);
        }

        [Fact]
        public void With_Configuration_Interceptor_Test()
        {
            var configuration = new AspectConfiguration();

            var configurationInterceptor = new InjectedInterceptor();
            configuration.GetConfigurationOption<IInterceptor>().Add(m => configurationInterceptor);

            var matcher = new InterceptorMatcher(configuration);
            var method = MethodInfoHelpers.GetMethod<Action<InterceptorMatcherModel>>(m => m.ConfigurationInterceptor());

            var interceptors = matcher.Match(method, method.DeclaringType.GetTypeInfo());

            Assert.NotEmpty(interceptors);

            Assert.Single(interceptors, configurationInterceptor);
        }

        [Fact]
        public void With_Method_Interceptor_Test()
        {
            var configuration = new AspectConfiguration();

            var matcher = new InterceptorMatcher(configuration);
            var method = MethodInfoHelpers.GetMethod<Action<InterceptorMatcherModel>>(m => m.WithInterceptor());

            var interceptors = matcher.Match(method, method.DeclaringType.GetTypeInfo());

            Assert.NotEmpty(interceptors);
            Assert.Single(interceptors);
        }


        [Fact]
        public void With_Type_Interceptor_Test()
        {
            var configuration = new AspectConfiguration();

            var matcher = new InterceptorMatcher(configuration);
            var method = MethodInfoHelpers.GetMethod<Action<WithInterceptorMatcherModel>>(m => m.WithOutInterceptor());

            var interceptors = matcher.Match(method, typeof(WithInterceptorMatcherModel).GetTypeInfo());

            Assert.NotEmpty(interceptors);
            Assert.Single(interceptors);
        }
    }
}
