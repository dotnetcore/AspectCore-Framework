using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AspectCore.Extensions.AspNetCore.Http
{
    public static class ExecutionContextHelper
    {
        /// <summary>
        /// 管理当前线程的执行上下文
        /// </summary>
        public static ExecutionContext Current { get; set; }
    }
}
