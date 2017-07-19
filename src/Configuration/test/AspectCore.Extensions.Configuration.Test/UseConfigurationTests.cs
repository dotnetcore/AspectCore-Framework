using AspectCore.Abstractions.Extensions;
using AspectCore.Abstractions.Internal;
using AspectCore.Extensions.Configuration.Test.Fakes;
using Xunit;

namespace AspectCore.Extensions.Configuration.Test
{
    public class UseConfigurationTests
    {
        [Fact]
        public void Use_Test()
        {
            var configure = new AspectConfigure();
            configure.Use<UseInterceptorAttribute>();
            var aspectValidator = new AspectValidator(configure);
            var method = ReflectionExtensions.GetMethod<IUserService>(nameof(IUserService.GetName));
            Assert.True(aspectValidator.Validate(method));
        }
    }
}
