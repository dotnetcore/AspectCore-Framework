using System;

namespace AspectCore.Lite.Abstractions.Resolution.Test.Fakes
{
    public class TargetService : ITargetService
    {
        public virtual int Add(int value)
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
}
