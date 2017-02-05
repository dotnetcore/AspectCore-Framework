using System;
using System.Reflection;
using Xunit;

namespace AspectCore.Abstractions.Resolution.Test
{
    public class AspectConfigureTest
    {
        [Fact]
        public void UseOption_Test()
        {
            var Configure = new AspectConfigure();
            var userOption = Configure.GetConfigureOption<IInterceptor>();
            Assert.NotNull(userOption);
            Assert.Empty(userOption);
            Func<MethodInfo, IInterceptor> defaultOption = m => default(IInterceptor);
            userOption.Add(defaultOption);
            Assert.Contains(defaultOption, userOption);
        }

        [Fact]
        public void IgnoreOption_Test()
        {
            var Configure = new AspectConfigure();
            var ignoreOption = Configure.GetConfigureOption<bool>();
            Assert.NotNull(ignoreOption);
            Assert.NotEmpty(ignoreOption);
        }
    }
}
