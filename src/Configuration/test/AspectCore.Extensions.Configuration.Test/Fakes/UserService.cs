using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Extensions.Configuration.Test.Fakes
{
    public class UserService : IUserService
    {
        public string GetName()
        {
            return "Test";
        }
    }
}
