using AspectCore.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace AspectCore.Abstractions.Internal.Test
{
    public class ConfigureOptionTest
    {
       [Fact]
        public void ConfigureOption_Add_Test()
        {
            IAspectConfigureOption<object> ConfigureOption = new AspectConfigure().GetConfigureOption<object>();
            Func<MethodInfo, object> option = m => new object();
            ConfigureOption.Add(option);
            Assert.Single(ConfigureOption, option);
        }
    }
}
