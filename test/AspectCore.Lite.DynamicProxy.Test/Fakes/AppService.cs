using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.DynamicProxy.Test.Fakes
{
    public class AppService : AbsAppService, IAppService, IAppService1
    {
        public override int Run(int arg)
        {
            return arg;
        }

        public override T Run1<T>(T arg)
        {
            return arg;
        }
    }

    public class AppService<T> : IAppService<T>, IAppService1<T>
    {
        public int Run(int arg)
        {
            return arg;
        }

        public T Run1(T arg)
        {
            return arg;
        }
    }
}
