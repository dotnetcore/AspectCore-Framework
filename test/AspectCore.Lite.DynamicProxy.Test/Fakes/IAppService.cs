using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.DynamicProxy.Test.Fakes
{
    public interface IAppService
    {
        int Run(int arg);
    }
}
