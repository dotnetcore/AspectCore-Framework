using AspectCore.Lite.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace AspectCore.Lite.Abstractions.Resolution.Test
{
    public class ConfigurationOptionTest
    {
       [Fact]
        public void ConfigurationOption_Add_Test()
        {
            IConfigurationOption<object> configurationOption = new AspectConfiguration().GetConfigurationOption<object>();
            Func<MethodInfo, object> option = m => new object();
            configurationOption.Add(option);
            Assert.Single(configurationOption, option);
        }
    }
}
