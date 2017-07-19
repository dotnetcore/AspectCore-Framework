using System;

namespace AspectCore.Abstractions.Internal.Test.Fakes
{
    public class InstanceServiceProvider : IServiceInstanceProvider, IServiceProvider
    {
        private readonly object instance;

        public InstanceServiceProvider(object instance)
        {
            this.instance = instance;
        }

        public object GetInstance(Type serviceType)
        {
            return instance;
        }

        public object GetService(Type serviceType)
        {
            return instance;
        }
    }
}