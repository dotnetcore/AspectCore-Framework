using System;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions.Resolution.Test.Fakes
{
    public class InjectedInterceptor : IInterceptor
    {
        [FromServices]
        public IAspectConfiguration Configuration { get; set; }

        [FromServices]
        public IAspectConfiguration ConfigurationWithNoSet { get; }

        public IAspectConfiguration ConfigurationWithNoFromServicesAttribute { get; set; }

        public bool AllowMultiple { get; }

        public int Order { get; set; }

        public Task Invoke(IAspectContext context, AspectDelegate next)
        {
            return next(context);
        }
    }
}
