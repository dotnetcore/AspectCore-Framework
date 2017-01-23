using System;
using System.Reflection;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public abstract class AspectActivatorContext
    {
        [Index(0)]
        public abstract Type ServiceType { get; }

        [Index(1)]
        public abstract MethodInfo ServiceMethod { get; }

        [Index(2)]
        public abstract MethodInfo TargetMethod { get; }

        [Index(3)]
        public abstract MethodInfo ProxyMethod { get; }

        [Index(4)]
        public abstract object TargetInstance { get; }

        [Index(5)]
        public abstract object ProxyInstance { get; }

        [Index(6)]
        public abstract object[] Parameters { get; }

        public class IndexAttribute : Attribute
        {
            public int Index { get; }
            public IndexAttribute(int index)
            {
                Index = index;
            }
        }
    }
}
