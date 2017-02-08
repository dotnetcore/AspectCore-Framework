using System;
using System.Reflection;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public abstract class AspectActivatorContext
    {
        public abstract Type ServiceType { get; }

        public abstract MethodInfo ServiceMethod { get; }

        public abstract MethodInfo TargetMethod { get; }

        public abstract MethodInfo ProxyMethod { get; }

        public abstract object TargetInstance { get; }

        public abstract object ProxyInstance { get; }

        public abstract object[] Parameters { get; }
    }
}
