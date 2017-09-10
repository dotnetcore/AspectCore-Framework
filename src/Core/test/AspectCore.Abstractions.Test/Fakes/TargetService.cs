using System;

namespace AspectCore.Abstractions.Internal.Test.Fakes
{
    public class TargetService : AbsTargetService, ITargetService
    {
        public override int Add(int value)
        {
            return value;
        }
    }

    public class TargetService<T> : ITargetService<T>
    {
        public T Add(T value)
        {
            throw new NotImplementedException();
        }
    }

    [Increment]
    public class AbsTargetService
    {
        public virtual int Add(int value)
        {
            return value;
        }
    }
}
