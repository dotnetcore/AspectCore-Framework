using System;
using System.Reflection;
using Xunit;

namespace AspectCore.Abstractions.Resolution.Test
{
    public class AspectConfigurationTest
    {
        [Fact]
        public void UseOption_Test()
        {
            var configuration = new AspectConfiguration();
            var userOption = configuration.GetConfigurationOption<IInterceptor>();
            Assert.NotNull(userOption);
            Assert.Empty(userOption);
            Func<MethodInfo, IInterceptor> defaultOption = m => default(IInterceptor);
            userOption.Add(defaultOption);
            Assert.Contains(defaultOption, userOption);
        }

        [Fact]
        public void IgnoreOption_Test()
        {
            var configuration = new AspectConfiguration();
            var ignoreOption = configuration.GetConfigurationOption<bool>();
            Assert.NotNull(ignoreOption);
            Assert.NotEmpty(ignoreOption);
        }
    }
}
