using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Abstractions.Resolution.Test.Fakes
{
    public class MatcherTestAttribute : InterceptorAttribute
    {
        public override Task Invoke(IAspectContext context, AspectDelegate next)
        {
            return next(context);
        }
    }

    public class MultipMatcherTestAttribute : InterceptorAttribute
    {
        public override bool AllowMultiple
        {
            get
            {
                return true;
            }
        }

        public override Task Invoke(IAspectContext context, AspectDelegate next)
        {
            return next(context);
        }
    }
}
