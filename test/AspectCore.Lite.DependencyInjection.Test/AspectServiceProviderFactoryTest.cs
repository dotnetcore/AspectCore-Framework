using System;
using  System.Linq.Expressions;
using Microsoft.AspNetCore.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.Lite.DependencyInjection.Test
{
    public class AspectServiceProviderFactoryTest
    {
        [Fact]
        public void Create_ThrowInvalidOperationException_Test()
        {
            var services = new ServiceCollection();
            ExceptionAssert.Throws<InvalidOperationException>(
                () => AspectServiceProviderFactory.Create(services.BuildServiceProvider()));
        }

        [Fact]
        public void Create_Test()
        {
            var services = new ServiceCollection();
            services.AddAspectLite();
            var provider = services.BuildServiceProvider();
            var proxyProvider = AspectServiceProviderFactory.Create(provider);
            Assert.NotNull(proxyProvider);
            Assert.NotEqual(provider, proxyProvider);
            var originalField = Expression.Field(Expression.Constant(proxyProvider), "originalServiceProvider");
            var originalServiceProvider = Expression.Lambda<Func<IServiceProvider>>(originalField).Compile()();
            Assert.Equal(provider, originalServiceProvider);
        }
    }
}