using System;

namespace AspectCore.Extensions.Test.Fakes
{
    public class Service : IService
    {
        public virtual Model Get(int id)
        {
            return new Model { Id = id, Version = Guid.NewGuid() };
        }
    }
}
