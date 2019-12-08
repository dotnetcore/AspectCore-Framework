using System.Collections.Generic;
using AspectCore.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AspectCore.Extensions.Configuration.Tests
{
    public class ConfigurationBindingTest
    {
        [Fact]
        public void LoadBinding()
        {
            var dict = new Dictionary<string, string>
            {
                {"creator:age", "24"},
                {"creator:name", "lemon"}
            };
            var builder = new ConfigurationBuilder().AddEnvironmentVariables();
            builder.AddInMemoryCollection(dict);
            var configuration = builder.Build();
            var container = new ServiceContext();
            container.AddInstance<IConfiguration>(configuration);
            container.AddConfigurationInject();
            container.AddType<BindConfigService>();
            var service = container.Build().Resolve<BindConfigService>();
            Assert.Equal(service.ToString(), "lemon-24");
        }
    }

    public class Config
    {
        public string Name { get; set; }
        
        public int Age { get; set; }
    }

    public class BindConfigService
    {
        [ConfigurationBinding("creator")]
        private Config _config;

        public override string ToString()
        {
            return $"{_config.Name}-{_config.Age}";
        }
    }
}