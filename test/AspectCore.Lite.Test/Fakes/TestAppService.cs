using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace AspectCore.Lite.Test.Fakes
{
    public class TestAppService : IAppService, IAppServiceA
    {
        private readonly ITestOutputHelper output;

        public TestAppService(ITestOutputHelper output)
        {
            this.output = output;
        }

        public int AppId { get; set; } = 2;

        public string AppName { get; set; } = "TestAppService";

        public void ExitApp()
        {
            output.WriteLine("TestAppService Exit.");
        }

        public string GetAppType()
        {
            return "testApp";
        }

        public bool RunApp(string[] args)
        {
            output.WriteLine("TestAppService Run.");
            return true;
        }
    }
}
