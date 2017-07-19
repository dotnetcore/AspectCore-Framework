using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Extensions.Configuration.Test.Fakes
{
    public interface IUserService
    {
        [ServiceInterceptor(typeof(LoggerInterceptorAttribute))]
        string GetName();
    }
}