using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.Abstractions;
using Xunit;

namespace AspectCore.Extensions.Configuration.Test.Fakes
{
    public class LoggerInterceptorAttribute : InterceptorAttribute
    {
        private readonly Logger logger;
        public LoggerInterceptorAttribute(Logger logger)
        {
            this.logger = logger;
        }

        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            Assert.NotNull(logger);
            return base.Invoke(context, next);
        }
    }
}
