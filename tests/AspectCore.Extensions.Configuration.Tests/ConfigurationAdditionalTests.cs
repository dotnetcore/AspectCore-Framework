using System;
using System.Collections.Generic;
using AspectCore.DependencyInjection;
using AspectCore.Extensions.Configuration;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AspectCore.Extensions.Configuration.Tests
{
    public class ConfigurationMetadataAttributeTests
    {
        [Fact]
        public void GetSection_NullSections_ReturnsNull()
        {
            var attr = new ConfigurationBindingAttribute(null);
            Assert.Null(attr.GetSection());
        }

        [Fact]
        public void GetSection_EmptySections_ReturnsNull()
        {
            var attr = new ConfigurationBindingAttribute();
            Assert.Null(attr.GetSection());
        }

        [Fact]
        public void GetSection_SingleSection_ReturnsSection()
        {
            var attr = new ConfigurationBindingAttribute("section1");
            Assert.Equal("section1", attr.GetSection());
        }

        [Fact]
        public void GetSection_MultipleSections_ReturnsJoinedSections()
        {
            var attr = new ConfigurationBindingAttribute("section1", "section2");
            Assert.Equal("section1:section2", attr.GetSection());
        }

        [Fact]
        public void GetSection_ValueAttribute_NullSections_ReturnsNull()
        {
            var attr = new ConfigurationValueAttribute("key", null);
            Assert.Null(attr.GetSection());
        }

        [Fact]
        public void GetSection_ValueAttribute_EmptySections_ReturnsNull()
        {
            var attr = new ConfigurationValueAttribute("key");
            Assert.Null(attr.GetSection());
        }

        [Fact]
        public void GetSection_ValueAttribute_SingleSection_ReturnsSection()
        {
            var attr = new ConfigurationValueAttribute("key", "section1");
            Assert.Equal("section1", attr.GetSection());
        }

        [Fact]
        public void GetSection_ValueAttribute_MultipleSections_ReturnsJoinedSections()
        {
            var attr = new ConfigurationValueAttribute("key", "section1", "section2");
            Assert.Equal("section1:section2", attr.GetSection());
        }
    }

    public class ServiceContainerExtensionsTests
    {
        [Fact]
        public void AddConfigurationInject_NullContext_ThrowsArgumentNullException()
        {
            IServiceContext context = null;
            Assert.Throws<ArgumentNullException>(() => context.AddConfigurationInject());
        }

        [Fact]
        public void AddConfigurationInject_RegistersCallback()
        {
            var container = new ServiceContext();
            container.AddConfigurationInject();
            var dict = new Dictionary<string, string>
            {
                {"creator:age", "24"},
                {"creator:name", "lemon"}
            };
            var builder = new ConfigurationBuilder().AddEnvironmentVariables();
            builder.AddInMemoryCollection(dict);
            var configuration = builder.Build();
            container.AddInstance<IConfiguration>(configuration);
            container.AddType<BindConfigService>();
            var service = container.Build().Resolve<BindConfigService>();
            Assert.Equal("lemon-24", service.ToString());
        }
    }

    public class ConfigurationBindResolveCallbackAdditionalTests
    {
        [Fact]
        public void Invoke_NullInstance_ReturnsNull()
        {
            var callback = new ConfigurationBindResolveCallback();
            var dict = new Dictionary<string, string> { { "key", "value" } };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
            var container = new ServiceContext();
            container.AddInstance<IConfiguration>(configuration);
            var resolver = container.Build().Resolve<IServiceResolver>();
            var result = callback.Invoke(resolver, null, null);
            Assert.Null(result);
        }

        [Fact]
        public void Invoke_IConfigurationInstance_ReturnsInstance()
        {
            var callback = new ConfigurationBindResolveCallback();
            var dict = new Dictionary<string, string> { { "key", "value" } };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
            var container = new ServiceContext();
            container.AddInstance<IConfiguration>(configuration);
            var resolver = container.Build().Resolve<IServiceResolver>();
            var result = callback.Invoke(resolver, configuration, null);
            Assert.Same(configuration, result);
        }
    }
}
