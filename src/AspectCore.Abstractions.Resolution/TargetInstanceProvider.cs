using System;

namespace AspectCore.Abstractions.Resolution
{
    public abstract class TargetInstanceProvider
    {
        public abstract object GetInstance(Type serviceType);
    }
}
