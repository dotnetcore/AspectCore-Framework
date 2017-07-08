using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Extensions.Test.Fakes
{
    public class Controller : IController
    {
        public IService Service { get; }

        public Controller(IService service)
        {
            Service = service;
        }

        public Model Execute()
        {
            return Service.Get(100);
        }
    }
}
