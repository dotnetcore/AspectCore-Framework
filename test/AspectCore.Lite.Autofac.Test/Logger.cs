using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Autofac.Test
{
    public class Logger
    {
        [ExecutionTimerInterceptor]
        public virtual void Info()
        {

        }
    }
}
