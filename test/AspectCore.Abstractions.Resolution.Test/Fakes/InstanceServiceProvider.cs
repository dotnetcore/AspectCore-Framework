using System;

namespace AspectCore.Abstractions.Resolution.Test.Fakes
{
    public class InstanceServiceProvider : TargetInstanceProvider, IServiceProvider
    {
        private readonly object instance;

        public InstanceServiceProvider(object instance)
        {
            this.instance = instance;
        }

        public override object GetInstance(Type serviceType)
        {
            return instance;
        }

        public object GetService(Type serviceType)
        {
            return instance;
        }
    }
}