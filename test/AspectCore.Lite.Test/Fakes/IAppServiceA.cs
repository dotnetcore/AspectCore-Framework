using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Test.Fakes
{
    public interface IAppServiceA
    {
        int AppId { get; set; }
        string AppName { get; set; }
        bool RunApp(string[] args);
        string GetAppType();
        void ExitApp();
    }
}
