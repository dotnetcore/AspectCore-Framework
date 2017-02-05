using System;
using System.Threading.Tasks;

namespace AspectCore.Abstractions.Resolution.Test.Fakes
{
    public class InjectedInterceptor : IInterceptor
    {
        [FromServices]
        public IAspectConfigure Configure { get; set; }

        [FromServices]
        public IAspectConfigure ConfigureWithNoSet { get; }

        public IAspectConfigure ConfigureWithNoFromServicesAttribute { get; set; }

        public bool AllowMultiple { get; }

        public int Order { get; set; }

        public Task Invoke(Abstractions.AspectContext context, AspectDelegate next)
        {
            return next(context);
        }
    }
}
