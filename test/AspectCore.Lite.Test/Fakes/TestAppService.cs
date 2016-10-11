using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Test.Fakes
{
    public class TestAppService : IAppService
    {
        public int AppId { get; set; } = 2;

        public string AppName
        {
            get; set;
        } = "TestAppService";

        public void ExitApp()
        {
            Console.WriteLine("TestAppService Exit.");

        }

        public string GetAppType()
        {
            return "testApp";
        }

        public bool RunApp(string[] args)
        {
            Console.WriteLine("TestAppService Run.{0}.", args);
            return true;
        }
    }
}
