using AspectCore.Lite.Abstractions;
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
