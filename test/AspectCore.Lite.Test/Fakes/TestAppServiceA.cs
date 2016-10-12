using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace AspectCore.Lite.Test.Fakes
{
    public class TestAppServiceA : IAppServiceA
    {
        public int AppId { get; set; } = 2;

        public string AppName { get; set; } = "TestAppService";

        public void ExitApp()
        {
        }

        [InterceptorApp]
        public virtual string GetAppType()
        {
            return "testAppA";
        }

        public bool RunApp(string[] args)
        {
            return true;
        }
    }
}
