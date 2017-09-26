using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Extensions.Autofac.WebSample
{
    public interface IValuesService
    {
        IEnumerable<string> GetAll();
    }

    public class ValuesService : IValuesService
    {
        public IEnumerable<string> GetAll()
        {
            return new string[] { "value", "value" };
        }
    }
}
