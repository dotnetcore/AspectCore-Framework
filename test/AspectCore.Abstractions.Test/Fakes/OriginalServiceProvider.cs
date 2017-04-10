using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Abstractions.Test.Fakes
{
    public class OriginalServiceProvider : IRealServiceProvider
    {
        private readonly object instance;
        public OriginalServiceProvider(object instance)
        {
            this.instance = instance;
        }

        public object GetService(Type serviceType)
        {
            return instance;
        }
    }
}
