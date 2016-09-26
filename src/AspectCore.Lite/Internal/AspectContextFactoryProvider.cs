using AspectCore.Lite.Core;
using System;

namespace AspectCore.Lite.Internal
{
    internal sealed class AspectContextFactoryProvider : IAspectContextFactoryProvider
    {
        public IAspectContextFactory ContextFactory
        {
            get
            {
                return new AspectContextFactory();
            }
        }
    }
}
