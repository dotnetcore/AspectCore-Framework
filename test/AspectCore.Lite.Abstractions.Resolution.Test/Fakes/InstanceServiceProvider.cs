using System;

namespace AspectCore.Lite.Abstractions.Resolution.Test.Fakes
{
    public class InstanceServiceProvider : IServiceProvider
    {
        private readonly object instance;

        public InstanceServiceProvider(object instance)
        {
            this.instance = instance;
        }

        public object GetService(Type serviceType)
        {
            return instance;
        }
    }
}
