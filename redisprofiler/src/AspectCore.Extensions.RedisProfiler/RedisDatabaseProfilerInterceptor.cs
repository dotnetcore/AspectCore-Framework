using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.RedisProfiler
{
    public class RedisDatabaseProfilerInterceptor : AbstractInterceptor
    {
        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            throw new NotImplementedException();
        }
    }
}