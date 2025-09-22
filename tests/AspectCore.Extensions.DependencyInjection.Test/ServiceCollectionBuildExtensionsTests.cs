using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.Extensions.DependencyInjection.Test;

public class ServiceCollectionBuildExtensionsTests
{
    [Fact]
    public void BuildDynamicProxyProvider_Validate()
    {
        var services = new ServiceCollection();
        var provider = services.BuildDynamicProxyProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
        Assert.NotNull(provider);
    }
}
