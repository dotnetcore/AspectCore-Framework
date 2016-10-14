using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Autofac.Test
{
    [ExecutionTimerInterceptor]
    public interface ITaskRepository
    {
        int Id { get; set; }
    }
}
