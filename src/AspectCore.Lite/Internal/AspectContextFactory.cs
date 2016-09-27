using AspectCore.Lite.Core;
using System;

namespace AspectCore.Lite.Internal
{
    internal class AspectContextFactory: IAspectContextFactory
    {
        private readonly Proxy proxy;
        private readonly Target target;
        public AspectContextFactory()
        {
        }

        public IAspectContext Create()
        {
            return new AspectContext(null , null , null , null , null);
        }
    }
}
