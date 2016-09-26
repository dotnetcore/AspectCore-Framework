using AspectCore.Lite.Core;
using System;

namespace AspectCore.Lite.Internal
{
    internal class AspectContextFactory: IAspectContextFactory
    {
        public AspectContextFactory()
        {
        }

        public AspectContext Create()
        {
            return new InternalAspectContext(null, null, null, null);
        }
    }
}
