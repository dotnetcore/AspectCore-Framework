using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Extensions.DependencyInjection.Sample.Web.Services
{
    public interface IHomeService
    {
        void Index();
    }

    public class HomeService : IHomeService
    {
        public void Index()
        {
            
        }
    }
}
