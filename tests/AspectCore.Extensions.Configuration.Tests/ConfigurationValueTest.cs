using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Xunit;
using  AspectCore.DependencyInjection;

namespace AspectCore.Extensions.Configuration.Tests
{
    public class ConfigurationValueTest
    {
        [Fact]
        public void LoadValue()
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
            container.AddType<ValueConfigService>();
            var service = container.Build().Resolve<ValueConfigService>();
            Assert.Equal(service.ToString(), "lemon-24");
        }
    }

    public class ValueConfigService
    {
        [ConfigurationValue("age", "creator")] 
        private int age;

        [ConfigurationValue("name", "creator")]
        private string name;

        public override string ToString()
        {
            return $"{name}-{age}";
        }
    }
}