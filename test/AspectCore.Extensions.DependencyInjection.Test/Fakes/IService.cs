using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Extensions.Test.Fakes
{
    public interface IService
    {
        [CacheInterceptor]
        Model Get(int id);
    }
}
